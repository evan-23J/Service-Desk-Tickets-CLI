# Service-Desk-Tickets-CLI

## About
A C# proyect that allows user to make CRUD operations on tickets depending on role. This program implements basic authentication and secure coding practices, such as password hashing, input validation, least privilege concepts, and has been developed with the Secure by Design concept.

## Authors
- Evan J. Tellado Rivera – etellado4727@interbayamon.edu
- Gian F. Rivera Martinez- grivera1912@interbayamon.edu

### Prerequisites
- .NET 6.0 SDK or higher
- Git installed
- Operating system: Windows, Linux, or macOS (developed and tested on windows.)


### How to Run
1. Clone the repository: https://github.com/evan-23J/Service-Desk-Tickets-CLI.git
2. Change directory: cd Service-Desk-Tickets-CLI  
3. Build the project: dotnet build
4. Run the project: dotnet run
5. The output will prompt you to choose an option, enter the number associated with that option and then enter your input
   Example:
   When you first run the program the following files will be created:   
   :userdata.json created succesfully  
   :tickets.json created succesfully  
   :logs.json created succesfully  
   
   userdata stores user information, tickets stores ticket entries and logs stores the actions taken by the users using the program.   
   
   (1) Register (2) LogIn (3) Exit  
   :1  
   Please enter a username:  
   :GitHubUser123  
   Please enter a password:  
   :SuperSecurePass!  
   User registered Succesfully!  

#### Threat Model
See `/docs/ThreatModel.pdf` for the threat model and documentation.

#### Security Audit

The project was audited using `dotnet list package --vulnerable`.
See `audit-screenshot.png`, results: The given project `ServiceDeskTickets` has no vulnerable packages given the current sources.


