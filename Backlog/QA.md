I) QA Objectives

Created automated testing for the following:

- The file upload process.
- The quiz generation logic.
- The handling of user roles and permissions.

All tests aim to validate the correct functionality of the UploadPdf method in the StudentsController by ensuring that PDF files are uploaded and processed correctly, quizzes are generated, and questions and answers are populated accurately.
Tested permissions, and verified that the system properly handles permission-related cases, where unauthorized users (e.g., students) cannot access restricted actions (e.g., editing professor details).

II) The testing process

1) Requirements Phase

Conducted Validation Testing to confirm functional requirements, such as quiz generation accuracy and results accuracy, and system constraints.

2) Design Phase

Analyzed the design of controllers, authentication, and database interaction.

3) Implementation Phase

Applied Unit Testing to verify individual methods (e.g., UploadPdf).
Wrote mock-based tests for services like UserManager and file uploads.

4) Integration Phase

Used Integration Testing to validate the flow between the Authentication Service, Database, and the core AI module.

5) System Testing

Conducted End-to-End Testing to ensure seamless user journeys for professors and students.


III) Testing Methods

1) Unit Testing 

- Validated individual components like StudentsController, methods like UploadPdf, and their logic.
- Verified correct interaction with services such as UserManager, SignInManager, and the database.

2) Integration Testing

- Ensured components work together correctly (e.g., controllers interacting with services and the database).
- Validated the interaction between the Authentication Service and user roles (Student, Professor).

3) System Testing

- Tested the system flow (e.g., uploading a PDF, extracting quiz questions, and quizzes have right answers).

4) Security Testing

- Authentication required for different parts of the application, and we get forbidden if we try different things
that he doesn't have authorization.

5) Validation Testing

-  Ensured alignment with business requirements, such as quiz generation accuracy and results accuracy.

IV) Results of testing 

Preliminary Observations:

1) Unit Testing

- Confirmed proper file handling in UploadPdf.
- Verified that quizzes have valid questions and answers.
- Detected edge cases (e.g., non-PDF uploads, empty files).

2) Integration Testing

- Verified no potential issues in how the Authentication Service handles session persistence for both roles.

3) System Testing

- The system flow works as intended.

4) Security Testing

Detected and removed gaps in unauthorized access.

5) Validation Testing

AI-based quiz generation meets basic accuracy and doesn't require further optimization to reduce errors in question extraction.

