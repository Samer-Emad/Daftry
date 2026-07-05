# Daftry (دفتري) - Financial Ledger & Retail Management System

Daftry is a robust, data-driven web application designed to digitalize retail transactions, manage customer accounts, track outstanding debts, and automate financial invoicing. Built using the **.NET ecosystem**, this system provides business owners with clear, real-time insights into their financial health.

## 🚀 Key Features

- **Digital Ledger System:** Comprehensive customer profile management with dynamic balancing of total purchases vs. total payments.
- **Dynamic Time-Based Filtering:** Built-in date filtering mechanics allowing users to generate live sales and debt statements by period (Daily, Weekly, Monthly, and Annual).
- **Automated PDF Reporting Engine:** Integrated a highly customized document compilation stream that fully supports Right-to-Left (RTL) Arabic text for rendering professional, on-the-spot customer invoices and multi-page financial reports.
- **Strict Data Integrity:** Engineered tailored database-level logic to manage complex, multi-tiered relationships (Customers ➡️ Orders ➡️ Items) safely, incorporating custom cascading resets.

## 🛠️ Tech Stack

- **Backend:** C# | ASP.NET Core MVC
- **Data Access:** Entity Framework Core (Code First)
- **Database:** Microsoft SQL Server
- **Reporting Engine:** QuestPDF (Advanced PDF generation & styling)
- **Frontend:** Bootstrap 5, HTML5, CSS3

## 📊 Database Architecture

The core relational structure centers around:
- **Customers:** Stores continuous profile metrics, overall debt, and balance flags.
- **Orders & OrderItems:** Captures individual sales, transactional serial numbers, quantities, and real-time unit prices.
- **Payments:** Manages continuous payment transactions linked back to the main ledger to deduct balances dynamically.

---
*Developed as a professional engineering project showcasing clean architecture, complex database relations, and advanced reporting integrations in .NET.*
