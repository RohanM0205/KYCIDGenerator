# 🔐 KYC ID Generator

A powerful, secure, and user-friendly web application for generating and managing **KYC (Know Your Customer)** records for **Individuals** and **Corporates**, with built-in support for **manual verification**, **document uploads**, and **admin approval workflows**.

🌐 **Live Website**: [infiniota.com]

---

## 🚀 Features

- ✅ **OTP-based Secure Login** for Users and Admins
- 👤 **Individual KYC** via PAN, Aadhaar, or Manual entry
- 🏢 **Corporate KYC** via PAN, GST, or Manual entry
- 📄 **Document Uploads** with Preview & File Validation
- 🛠 **Admin Dashboard** with:
  - 📊 Filterable KYC views (All, Approved, Pending, Rejected)
  - 🔍 Search, pagination, and sorting
  - ✔️ Approve or ❌ Reject functionality with reason logging
- 📂 **Rejection Reason Handling** with manual reason entry
- 🧾 Tracks **Created Date**, **Status Modified Date**, **Create Type** (Auto/Manual)
- 📦 Integrated with **SQL Server** using Entity Framework

---

## 🛠 Technologies Used

| Technology              | Purpose                                 |
|------------------------|-----------------------------------------|
| ASP.NET Core MVC       | Backend Framework                       |
| Entity Framework Core  | ORM & Data Access Layer                 |
| SQL Server             | Relational Database                     |
| Bootstrap 5            | UI Styling & Responsive Layouts         |
| Razor Pages            | View Layer                              |
| LINQ                   | Querying Data                           |
| JavaScript / jQuery    | Interactivity, File Preview, UX         |
| Git & GitHub           | Version Control                         |

---

## 📁 Folder Structure

/KYCIDGenerator
│
├── Controllers/ → MVC Controllers
├── Models/ → ViewModels & Entity Classes
├── Views/ → Razor Views (.cshtml)
│ ├── Admin/
│ ├── Kyc/
│ └── Shared/
├── wwwroot/ → Static files (JS, CSS, Docs)
├── Migrations/ → EF Core migration scripts
└── appsettings.json → Configuration & Connection Strings


---

## 🔑 Admin Dashboard Preview

The admin can:

- Filter by Customer Type, Create Type, Status, and Dates
- Search KYC records by ID or Name
- View full KYC record and attached documents
- Approve or Reject with one click

---

## 📌 Setup Instructions

> Prerequisite: .NET 8 SDK, Visual Studio 2022+, SQL Server

```bash
git clone https://github.com/RohanM0205/KYCIDGenerator.git
cd KYCIDGenerator
dotnet restore
dotnet ef database update  # Apply migrations
dotnet run



🤝 Contributing
Pull requests are welcome! For major changes, please open an issue first to discuss what you would like to change.

⚖️ License
This project is licensed under the MIT License.

💬 Support or Feedback?
If you face any issues or have feature requests, feel free to open an issue or contact the maintainer.