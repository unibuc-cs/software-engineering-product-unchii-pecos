document.addEventListener('DOMContentLoaded', function () {
    const singleplayerBtn = document.getElementById('singleplayerBtn');
    const multiplayerBtn = document.getElementById('multiplayerBtn');
    const materialsQuestion = document.getElementById('materialsQuestion');
    const myMaterialsBtn = document.getElementById('myMaterials');
    const yourMaterialsBtn = document.getElementById('yourMaterials');
    const materialsForm = document.getElementById('materialsForm');
    const multiplayerChoice = document.getElementById('multiplayerChoice');
    const joinRoomBtn = document.getElementById('joinRoom');
    const hostRoomBtn = document.getElementById('hostRoom');
    const joinGameForm = document.getElementById('joinGameForm');

    function toggleGameMode(selectedMode) {
        if (selectedMode === 'singleplayer') {
            materialsQuestion.style.display = 'block';
            multiplayerChoice.style.display = 'none';
            singleplayerBtn.classList.add('btn-active');
            multiplayerBtn.classList.remove('btn-active');
            joinGameForm.style.display = 'none';
        } else if (selectedMode === 'multiplayer') {
            materialsQuestion.style.display = 'none';
            multiplayerChoice.style.display = 'block';
            multiplayerBtn.classList.add('btn-active');
            singleplayerBtn.classList.remove('btn-active');
            materialsForm.style.display = 'none';
        }
    }

    singleplayerBtn.addEventListener('click', function () {
        toggleGameMode('singleplayer');
    });

    multiplayerBtn.addEventListener('click', function () {
        console.log('Multiplayer button clicked');
        toggleGameMode('multiplayer');
    });

    joinRoomBtn.addEventListener('click', function () {
        this.classList.add('btn-active');
        hostRoomBtn.classList.remove('btn-active');
        joinGameForm.style.display = 'block';
    });

    hostRoomBtn.addEventListener('click', function () {
        this.classList.add('btn-active');
        joinRoomBtn.classList.remove('btn-active');
        joinGameForm.style.display = 'none';
    });

    myMaterialsBtn.addEventListener('click', function () {
        this.classList.add('btn-active');
        yourMaterialsBtn.classList.remove('btn-active');
        materialsForm.style.display = 'block';
    });

    yourMaterialsBtn.addEventListener('click', function () {
        this.classList.add('btn-active');
        myMaterialsBtn.classList.remove('btn-active');
        materialsForm.style.display = 'none';
    });
    

    multiplayerChoice.addEventListener('submit', async (e) => {
        e.preventDefault();
        console.log("am ajuns aici");
        const code = document.getElementById('materiiDropdown').value;
        console.log(code);
        // CreateRoom endpoint
        const response = await fetch('/Rooms/CreateRoom', {
            method: 'POST',
            headers: {'Content-Type': 'application/json'},
            body: JSON.stringify({ Code: code })
        });
        const data = await response.json();

        if (data.code) {
            window.location.href = `/Rooms/Lobby?code=${data.code}`;
            connection.invoke('JoinRoomGroup', data.code);
        } else {
            // :(
            alert('Could not create room');
        }
    })

    joinGameForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const code = document.getElementById('gameCode').value;

        const response = await fetch('/Rooms/JoinRoom', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ code })
        });

        const data = await response.json();
        if (data.success) {
            //alert(data.message);
            window.location.href = `/Rooms/Lobby?code=${code}`;
            connection.invoke("JoinRoomGroup", code);
        } else {
            alert(data.message);
        }
    });

    document.getElementById("startGameBtn").addEventListener("click", async () => {
        // Trimitem solicitarea către server pentru a începe jocul
        try {
            connection.on("StartGame", (questions) => {
                startQuestion(questions, 0);  // Start the first question
            });
            console.log("Questions received:", questions);
        } catch (err) {
            console.error("Error starting game:", err);
        }
    });

    // Function to display question and handle timer
    function startQuestion(questions, questionIndex) {
        if (questionIndex >= questions.length) {
            // All questions are done, get results
            connection.invoke("GetResults", code).then((data) => {
                displayResults(data.results);
            });
            return;
        }

        const question = questions[questionIndex];
        const questionDiv = document.getElementById("questionDiv");
        questionDiv.innerHTML = `
        <h3>${question.text}</h3>
        <ul id="answersList">
            ${question.answers.map((answer, index) => `<li data-answer="${answer}">${answer}</li>`).join('')}
        </ul>
        <p id="timer"></p>
    `;

        const timerElement = document.getElementById("timer");
        let timeLeft = 30;  // 30 second timer

        const timerInterval = setInterval(() => {
            timeLeft--;
            timerElement.textContent = `${timeLeft} seconds remaining`;
            if (timeLeft <= 0) {
                clearInterval(timerInterval);
                submitAnswer(questionIndex, null);  // Automatically submit if no answer
            }
        }, 1000);

        // Collect answer when user clicks
        document.querySelectorAll("#answersList li").forEach(item => {
            item.addEventListener("click", () => {
                clearInterval(timerInterval);
                submitAnswer(questionIndex, item.dataset.answer);
            });
        });

        // Function to submit answer
        function submitAnswer(questionIndex, answer) {
            connection.invoke("SubmitAnswer", {
                RoomCode: code,
                QuestionID: questions[questionIndex].id,
                Answer: answer || "",
                StudentId: userId
            });
            startQuestion(questions, questionIndex + 1);  // Move to the next question
        }
    }

    // Display results
    function displayResults(results) {
        const resultsDiv = document.getElementById("resultsDiv");
        resultsDiv.innerHTML = results.map(result => `
        <p>${result.Participant}: ${result.CorrectAnswers} correct answers</p>
    `).join('');
    }


    const loadingScreen = document.getElementById("loadingScreen");
    const loadingMessage = document.getElementById("loadingMessage");

    const messages = [
        "Your PDF is being processed...",
        "Cooking the questions...",
        "Almost ready...",
        "Preparing your quiz..."
    ];

    let messageIndex = 0;

    function showLoadingScreen() {
        loadingScreen.style.display = "flex";
        updateLoadingMessage();
    }

    function hideLoadingScreen() {
        loadingScreen.style.display = "none";
    }

    function updateLoadingMessage() {
        messageIndex = 0; // Reset to the first message
        loadingMessage.textContent = messages[messageIndex];

        const interval = setInterval(() => {
            messageIndex = (messageIndex + 1) % messages.length;
            loadingMessage.textContent = messages[messageIndex];
        }, 3000);

        // Stop updating the messages after 10 seconds
        setTimeout(() => clearInterval(interval), 10000);
    }

    // Example: Attach the loading screen to a button click and simulate the POST request
    document.getElementById("uploadpdf-submit").addEventListener("click", function () {
        showLoadingScreen();
    });
});