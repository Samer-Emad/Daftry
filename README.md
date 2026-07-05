# 📒 Daftry (دفتري) – Financial Ledger & Retail Management System

Daftry is an enterprise-grade financial ledger and retail management system developed to digitize traditional customer account tracking and retail operations. The application enables business owners to manage customer debts, sales, payments, and generate professional invoices with real-time financial calculations.

Built using **ASP.NET Core MVC** and **Entity Framework Core**, the system follows clean architecture principles with Repository Pattern and ViewModels to ensure maintainability and scalability.

---

## 🚀 Features

### 💰 Customer Financial Ledger

- Real-time calculation of customer balance based on total purchases and total payments.
- Dynamic financial statements with period filtering:
  - Daily
  - Weekly
  - Monthly
  - Yearly
  - All Transactions
- Customer profile management with complete ledger history.

---

### 🛒 Orders Management

- Create and manage customer orders.
- Generate unique order serial numbers.
- Store complete order history.
- Track product price history per order.
- Automatic total calculations.

---

### 💳 Payments Management

- Record customer payments.
- Automatically update remaining balance.
- Keep full payment history.
- Real-time debt calculations.

---

### 📄 PDF Invoice Generation

Using **QuestPDF**, the system generates professional invoices and reports including:

- Individual customer invoices.
- Complete customer statements.
- Period-based reports.
- RTL (Right-to-Left) Arabic support.
- Automatic system print timestamp.
- Multi-page PDF generation.

---

### 🗄️ Database Integrity

The application maintains relational integrity between entities through controlled deletion flow.

When deleting a customer, the system automatically removes:

1. Order Items
2. Orders
3. Payments
4. Customer Record

This prevents orphaned records and preserves database consistency.

---

## 🏗️ Tech Stack

| Technology | Description |
|------------|-------------|
| ASP.NET Core MVC | Web Framework |
| .NET 8 | Backend Platform |
| Entity Framework Core | ORM |
| SQL Server | Database |
| QuestPDF | PDF Generation |
| Bootstrap 5 | Frontend UI |
| HTML5 / CSS3 / JavaScript | Frontend |
| Repository Pattern | Data Access |
| ViewModels | Presentation Layer |

---

## 📊 Database

### Entities

### Customer

- Name
- Phone
- Orders
- Payments

### Orders

- Order Serial Number
- Order Date
- Customer
- Order Items

### Order Items

- Product Name
- Quantity
- Unit Price

### Payments

- Amount
- Payment Date

---

## ⚙️ Business Logic Highlights

### Dynamic Period Filtering

The application supports dynamic filtering of customer transactions by:

- Today
- Last 7 Days
- Current Month
- Current Year
- Entire History

using LINQ with custom filtering logic.

---

### Real-Time Balance Calculation

Customer balance is calculated automatically:

```text
Balance = Total Purchases − Total Payments
```

No manual calculations are required.

---

### Invoice Engine

The invoice module:

- Generates printable PDF invoices.
- Displays product details.
- Shows quantities and prices.
- Includes transaction date.
- Includes print timestamp.
- Supports Arabic RTL formatting.

---


## 👨‍💻 Author

**Samer Emad**

Back-End ASP.NET Core Developer

- GitHub: https://github.com/Samer-Emad
- LinkedIn: https://www.linkedin.com/in/samer-emad-se1112004/
