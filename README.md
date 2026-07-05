# Daftry (دفتري) - Financial Ledger & Retail Management System

Daftry is an enterprise-grade, data-driven web application engineered to digitalize traditional retail operations, track customer ledger accounts, and manage automated financial transactions. Developed using **ASP.NET Core MVC** and **Entity Framework Core**, this system provides business owners with clear, real-time tracking of dynamic customer balances, sales periods, and instant invoice printing.

---

## 🛠️ Tech Stack & Architecture

- **Framework:** .NET 8.0 (ASP.NET Core MVC)
- **Data Access Layer:** Entity Framework Core (Code-First Approach)
- **Database:** Microsoft SQL Server
- **Reporting Engine:** QuestPDF (Advanced Document Layout & PDF Generation Engine)
- **Frontend UI:** Bootstrap 5, HTML5, CSS3, JavaScript
- **Design Patterns:** Repository Pattern (Abstraction Layer) & ViewModels for clean data separation.

---

## 🚀 Detailed Features & Implementation Specs

### 1. Advanced Financial Ledger (Customers Controller)
- **Real-Time Statement Calculus:** The system aggregates total historical purchases against total payments made to output a live net debt/balance status.
- **Dynamic Period Filtering:** Engineered a custom time-windowing logic (`AsEnumerable()` to `Where` filtering) allowing instant reports based on:
  - **Day:** Tracks current calendar date.
  - **Week:** Looks back exactly 7 days.
  - **Month:** Aggregates transactions from the 1st of the current month.
  - **Year:** Full multi-quarter tracking starting from January 1st of the current year.
- **Data Safeguards:** Handled clean data mutation in HTTP POST actions (e.g., explicitly ensuring property assignments like `.Name` and `.Phone` map correctly to prevent database corruption).

### 2. Multi-Tiered Database Integrity & Cascade Resets
- Managed deep relational constraints across 4 main entities: `Customer ➡️ Orders ➡️ OrderItems & Payments`.
- **Manual Cascading Deletion:** To prevent orphaned records and protect DB referential integrity, the system implements a strict clean-up sequence when deleting a customer profile:
  1. Identifies and removes all nested `OrderItems` linked via foreign keys.
  2. Clears out the parent `Orders`.
  3. Purges all historical `Payments`.
  4. Finally commits the `Customer` deletion via transactional `SaveChangesAsync()`.

### 3. Automated Document Generation Engine (Orders Controller)
- **Single Invoice Compilation (`PrintInvoice`):** Generates standalone customer receipts rendering accurate unit pricing, quantities, original transaction timestamps, alongside a live **"System Print Timestamp"** to ensure document traceability.
- **Accumulated Statements Reporting (`PrintAllInvoices` / `PrintPeriodInvoices`):** Streamlines multi-page PDF rendering. It loops through deep-nested structures (`.Include(x => x.Orders).ThenInclude(x => x.Items)`) to draw high-performance sub-tables for each distinct order inside a single PDF stream.
- **RTL & Arabic Typography:** Configured QuestPDF layout infrastructure to handle **Right-to-Left (RTL)** flow seamlessly, ensuring professional, aligned Arabic invoice generation without formatting issues.

---

## 📊 Database Schema Blueprint
