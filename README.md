Project Overview: BeverageManagement

Solution Structure - 
    The BeverageManagement solution consists of one startup project and three supporting class libraries:

Startup Project: BeverageManagement
Class Libraries:
  Common – Contains class, interface.
  DalLayer – Responsible for data access logic and communication with the database.
  Model – Contains entity classes and potentially view models used across the solution.

A. Core Features
  This application provides functionality for managing:
  1. CRUD operations on:
      Beverage Categories
      Beverage Details

  2. Order Placement functionality

B. System Design & Architecture Highlights
 1. Authorization – Basic role-based access control implemented.
 2.Validation – Input validation in both frontend and backend layers.
 3.Exception Handling – Centralized error handling approach in place.
 4.Database Scripts – SQL script (BeverageManagement.sql) included for schema and seed data setup.

C. Configuration Requirements
-To run the application:
    Update the database connection string in appsettings.json:
    "ConnectionStrings": {
        "DefaultConnection": "<Your_Connection_String>"
      }
- Execute the BeverageManagement.sql script to set up the database.

D. Scope for Improvement with respect to current development
1.  Introduction of DTOs (Data Transfer Objects)
    Current Issue: UI and backend are tightly coupled with the model layer.

E. Improvement: Use DTOs to decouple UI from internal domain models. This provides better control over exposed data and enhances security and maintainability.

2. Centralized Error Handling Framework
3. Enhanced Input Validation
