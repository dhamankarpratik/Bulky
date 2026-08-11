<img width="1280" height="779" alt="image" src="https://github.com/user-attachments/assets/0db5289d-bfec-448a-a5f5-177f62ad2d36" /># Bulky Book Store

A full-stack e-commerce web application built using ASP.NET Core MVC. The application allows customers to browse products, add items to the cart, place orders, and securely make online payments. It also includes an admin panel for managing products, categories, companies, users, and orders.

## Acknowledgements

This project is based on the **ASP.NET Core MVC** course by **Bhrugen Patel** on Udemy. In addition to completing the course implementation, I integrated Razorpay for payment processing and made other modifications for learning and practice.

## Features

### Customer
- User Registration & Login
- Email Confirmation
- Product Browsing
- Shopping Cart
- Checkout Process
- Order History
- Razorpay Payment Integration

### Admin
- Dashboard
- Category Management
- Product Management
- Company Management
- User Management
- Role-Based Authorization
- Order Management
- Order Status Updates
- Shipping Management

## Technologies Used

- ASP.NET Core MVC (.NET 8)
- Entity Framework Core
- SQL Server
- ASP.NET Core Identity
- Repository Pattern
- Unit of Work Pattern
- Razor Views
- Bootstrap 5
- jQuery
- DataTables
- Razorpay Payment Gateway

## Architecture

- N-Tier Architecture
- Repository Pattern
- Unit of Work Pattern
- Dependency Injection

## Project Structure

Bulky
├── Bulky.DataAccess
├── Bulky.Models
├── Bulky.Utilities
├── BulkyWeb             # Main ASP.NET Core MVC application
├── BulkyWebRazor        # Separate Razor Pages practice project
└── README.md


## Screenshots

- Home Page
 <img width="1280" height="779" alt="image" src="https://github.com/user-attachments/assets/561b3ef4-8ca6-4687-b9c0-eb72137cd4a7" />
- Product Details
 <img width="1278" height="768" alt="image" src="https://github.com/user-attachments/assets/6a354899-456d-4d99-95bf-e144217f4d7e" />
- Shopping Cart
  <img width="1277" height="780" alt="image" src="https://github.com/user-attachments/assets/dc95b3b1-a89a-4146-a1ba-9c9813d90f44" />
- Checkout
  <img width="1280" height="772" alt="image" src="https://github.com/user-attachments/assets/f55b1b2d-03da-4625-a9c1-399fc54a3fb9" />
  <img width="1280" height="779" alt="image" src="https://github.com/user-attachments/assets/c0b52f77-1739-47b7-9702-097baa9ed5bf" />
  <img width="1271" height="760" alt="image" src="https://github.com/user-attachments/assets/eec14e24-0c85-4198-9727-b516d6d4ffab" />
  <img width="1277" height="763" alt="image" src="https://github.com/user-attachments/assets/2e3ede40-56b6-4de2-988c-1fc438f75d5a" />
  <img width="1280" height="764" alt="image" src="https://github.com/user-attachments/assets/dec4af0b-4c92-44cd-9eb6-549995f0135d" />
- Admin Dashboard
  <img width="1280" height="766" alt="image" src="https://github.com/user-attachments/assets/a0cd1300-320f-4a46-b35e-034293747090" />

- Product Management
  <img width="1280" height="764" alt="image" src="https://github.com/user-attachments/assets/c5b6ef8f-efe1-40e5-9e80-05838ceac86d" />

- Order Management
  <img width="1277" height="770" alt="image" src="https://github.com/user-attachments/assets/5bded22e-dbcd-4755-8368-55bdd5cb4876" />
  <img width="1280" height="767" alt="image" src="https://github.com/user-attachments/assets/a0dbd4ca-3533-42c1-ab5a-db114650835d" />
  <img width="1280" height="762" alt="image" src="https://github.com/user-attachments/assets/4e4682a7-1f08-4e95-9f70-1281b274eff2" />


## Getting Started

## Prerequisites
-.NET 8 SDK
-SQL Server
-Visual Studio 2022 or another compatible IDE
-Razorpay Test Mode account (for payment testing)

##Configuration
-Clone the repository.
-Configure the SQL Server connection string in appsettings.json.
-Configure Razorpay Test Mode credentials locally.
-Apply Entity Framework Core migrations.
-Build and run the BulkyWeb project.

##Database
-The application uses Entity Framework Core Code First with SQL Server.
-Update the connection string in appsettings.json before running the application.

## Payment Gateway

This project uses **Razorpay** for online payment processing.

For security, Razorpay credentials are **not included in this repository**.

Configure your Razorpay Test Mode credentials locally:

```json
"RazorPay": {
  "KeyId": "YOUR_RAZORPAY_KEY_ID",
  "SecretKey": "YOUR_RAZORPAY_SECRET_KEY"
}
```
## Future Improvements

- Product Reviews
- Wishlist
- Coupon System
- Product Search
- Email Notifications
- Invoice Generation
- Sales Reports

## Learning Outcomes

Through this project, I gained hands-on experience with:

- ASP.NET Core MVC
- Entity Framework Core
- ASP.NET Core Identity
- Authentication & Authorization
- Role-Based Access Control
- Repository & Unit of Work Patterns
- Dependency Injection
- Razorpay Payment Integration
- CRUD Operations
- Session & Cookies
- File Uploads
- DataTables
- SQL Server Database Integration
- Application Configuration

## Author

Pratik Dhamankar
