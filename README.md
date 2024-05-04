# Expense Tracker
A small project built with the intention of learning how to use GRPC on `.NET 7/8`. The project is just a small practice app
that takes advantages of the features added to the most recent versions of `.NET` (pattern matching, minimal APIs, records).

It's been a while since the last time I actually worked on a `.NET` application and this project allowed me to recover my
confidence with my `.NET` skills and learn how to handle the new features added to the framework and the `C#` language.

## Components
The project is currently composed of two services, a base **ExpenseTracker** service and an **AuthService**. The whole thing is still a work in progress so it will keep on growing on the following days, weeks and months. Right now the name of every service pretty much describes its responsibility within the system, so its not difficult to figure out what role is played by each service.

A third service will be eventually introduced to project, said service will probably work as an **APIGateway** that will handle most of the requests and perform the necessary aggregations for each one of them.

A library project on charge of validating JWTs, is also part of the system. Said project is distributed among all the services so they all can handle Auth by themselves.

Unit tests will be eventually added to the project as well in the future.

## Interesting things I have in mind (the list might grow in the future)
* Use `F#` instead of `C#` for the **ExpenseTracker** service.