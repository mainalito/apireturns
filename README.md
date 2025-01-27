# 📊 High-Performance Returns Processing API

## Overview
This is a **C# ASP.NET REST API** designed to efficiently process **Excel files** in the **Returns module**. It:
- Accepts **Excel files** from entities.
- Converts the data to **CSV** format.
- Uses **BULK INSERT** to efficiently store over **12 million rows** into the **SQL Server database**.
- **Migrated from PHP** due to performance issues handling large datasets.

🚀 **Built for enterprise-scale data processing with high concurrency support.**

---

## 🔧 **Tech Stack**
| Technology | Purpose |
|------------|---------|
| **C# (.NET 8)** | High-performance backend |
| **ASP.NET Core Web API** | RESTful API implementation |
| **EPPlus** | Excel file handling |
| **ADO.NET (SQL Server)** | Database connectivity |
| **BULK INSERT** | Optimized for large-scale data ingestion |

---
---

## 📥 **API Endpoints**
### **1️⃣ Upload and Process an Excel File**
#### **Endpoint**
```http
POST /api/values
