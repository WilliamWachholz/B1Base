# B1Base

B1Base is a library to develop for SAP Business One. It saves a lot of effort since abstracts some primarly work of SAP B1 development.

B1Base is currently built with visual studio 2017 and targets .net 4.5

# Key Features

* Based on MVC concept.
* MainController to control Application basics (e.g addOn initialize/finalize, event handler, menu creation)
* BaseView to control UI API basics.
* ConnectionController to control DI API Basics together with Models and DAOs of DI API Objects.
* Support both HANA and SQL Server. 
* Queries can be changed directly in the addOn installation directory.
* Easy grouping of addOns created separately.

# Development
B1Base is developed with the following workflow:

* Someone needs it to do something it doesn't already do, that person implements that something and submits a pull request
* If it doesn't have a feature that you want it to have, add it.  If it has a bug you need fixed, fix it.

## Contribute:
All contributions welcome.  I'll try to respond the same day to any emails or pull requests.  Or within a few 
days at the most.  Small pull requests are best as they are easier to review.

### Core Team

* [Wachholz](https://github.com/WilliamWachholz)

## TO-DO:
* Create checkbox on configView named "Log SQL" (if checked keep executed queries in a subdirectory of addon installation folder)
* Verify CheckAll, don't use SetValue for performance reasons
* Create a method on BaseView to create new controls in System Forms using xml and BatchActions, to avoid flickering
* See if can be used direct bound on BaseView, for performance reasons
* Implement DAO object for B1ServiceLayer or DI Server communication


