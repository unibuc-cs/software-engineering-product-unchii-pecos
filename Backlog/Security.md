
Security Analysis

The app is divided into two components: the website, which contains most of the application's functionality, and the web service (an in-house API) hosted at https://api.fminatorul.xyz in Nginx. This API is responsible for generating quizzes using AI.

We have carefully considered security measures for both components.

API:
A secure HTTPS connection is established between the API and the website, ensuring all traffic is encrypted. This is achieved by hosting the API behind an Nginx server on Google Cloud.
JSON Web Tokens (JWT) are used for authentication between the API and the website. This ensures that only our services can access the API.
API testing is secured with a login mechanism that requires a username and password.
When the website connects to the API, a short-lived JWT with a 1-minute expiration is created. This ensures the API is used only for the specific task at hand and reduces the risk of misuse.

Website:
The app supports three types of users: Admin, Student, and Professor. Each role has specific privileges and access restrictions. Users cannot access features outside their assigned role.
User registration is limited to email domains such as s.unibuc.ro (students) or unibuc.ro (professors), ensuring only authorized individuals can create accounts.
Comprehensive tests have been implemented for all controller endpoints to ensure users can only access the features permitted for their roles.
