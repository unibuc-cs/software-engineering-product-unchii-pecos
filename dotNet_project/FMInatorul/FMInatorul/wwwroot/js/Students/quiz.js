// quiz.js

document.addEventListener('DOMContentLoaded', function () {
    const questionItems = document.querySelectorAll('.question-item');
    const navItems = document.querySelectorAll('.question-nav-item');

    // Add event listeners to radio buttons
    questionItems.forEach((questionItem, index) => {
        const radioButtons = questionItem.querySelectorAll('input[type="radio"]');
        radioButtons.forEach(radio => {
            radio.addEventListener('change', function () {
                // Mark question as answered
                if (radio.checked) {
                    navItems[index].classList.add('answered');
                }
                // Scroll to next question
                if (index + 1 < questionItems.length) {
                    questionItems[index + 1].scrollIntoView({ behavior: 'smooth' });
                }
            });
        });
    });

    // Add event listeners to 'Clear Selection' buttons
    const clearButtons = document.querySelectorAll('.clear-selection');
    clearButtons.forEach((button, index) => {
        button.addEventListener('click', function () {
            const radioButtons = questionItems[index].querySelectorAll('input[type="radio"]');
            radioButtons.forEach(radio => {
                radio.checked = false;
            });
            // Mark question as unanswered
            navItems[index].classList.remove('answered');
        });
    });

    // Add event listeners to nav items
    navItems.forEach((navItem, index) => {
        navItem.addEventListener('click', function () {
            // Scroll to the question
            questionItems[index].scrollIntoView({ behavior: 'smooth' });
        });
    });

    // Update active question in sidebar based on scroll
    const observerOptions = {
        root: document.querySelector('.main-content'), // Set the root to the scrolling element
        rootMargin: '0px',
        threshold: 0.6
    };

    const observer = new IntersectionObserver(entries => {
        entries.forEach(entry => {
            const index = Array.from(questionItems).indexOf(entry.target);
            if (entry.isIntersecting) {
                navItems[index].classList.add('active');
            } else {
                navItems[index].classList.remove('active');
            }
        });
    }, observerOptions);

    questionItems.forEach(questionItem => {
        observer.observe(questionItem);
    });


    const mainContent = document.querySelector('.main-content');
    const sidebarNav = document.querySelector('.question-nav');

    // Synchronize scrolling between main content and sidebar
    mainContent.addEventListener('scroll', function () {
        const contentScrollTop = mainContent.scrollTop;
        const contentScrollHeight = mainContent.scrollHeight - mainContent.clientHeight;
        const scrollPercentage = contentScrollTop / contentScrollHeight;

        const sidebarScrollHeight = sidebarNav.scrollHeight - sidebarNav.clientHeight;
        sidebarNav.scrollTop = scrollPercentage * sidebarScrollHeight;
    });


    const submitButton = document.querySelector('.submit-button');
    //const questionItems = document.querySelectorAll('.question-item');

    function validateAnswers() {
        let allAnswered = true;

        questionItems.forEach(question => {
            const radioButtons = question.querySelectorAll('input[type="radio"]');
            const isAnswered = Array.from(radioButtons).some(radio => radio.checked);
            if (!isAnswered) {
                allAnswered = false;
            }
        });

        // Enable or disable the submit button based on validation
        submitButton.disabled = !allAnswered;
    }

    // Validate on input change
    questionItems.forEach(question => {
        const radioButtons = question.querySelectorAll('input[type="radio"]');
        radioButtons.forEach(radio => {
            radio.addEventListener('change', validateAnswers);
        });
    });

    // Initial validation on page load
    validateAnswers();
});
