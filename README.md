# Agri Connect Microservices Architecture

An agricultural platform consisting of a **Next.js 15** frontend, a **Node.js Express** API Gateway, and four dedicated **ASP.NET Core Web API** microservices.

---

## 🚀 Port Allocation Matrix

| Service | Folder | Technology | Port | Direct URL | Gateway Route |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Frontend UI** | `agri-ui` | Next.js 15 (React 19, Tailwind) | **3000** | `http://localhost:3000` | — |
| **API Gateway** | `api-gateway` | Node.js + Express + Proxy | **5000** | `http://localhost:5000` | `/health`, `/` |
| **Admin Service** | `admin service` | ASP.NET Core Web API (.NET 10/8) | **5001** | `http://localhost:5001` | `/api/admin/*` |
| **Farmer Service** | `farmer service` | ASP.NET Core Web API (.NET 10/8) | **5002** | `http://localhost:5002` | `/api/farmers/*` |
| **Officer Service** | `officer service` | ASP.NET Core Web API (.NET 10/8) | **5003** | `http://localhost:5003` | `/api/officers/*` |
| **Supplier Service**| `supplier service`| ASP.NET Core Web API (.NET 10/8) | **5004** | `http://localhost:5004` | `/api/suppliers/*` |

---

## ⚡ Quick Start (Run All Services)

You can launch all services in separate terminal windows with a single command:

### Option A: Command Prompt / Windows Run
Double-click `start-all.bat` or run:
```cmd
.\start-all.bat
```

### Option B: PowerShell
```powershell
.\start-all.ps1
```

---

## 🛠️ Running Services Individually

### 1. Agri UI (Next.js Frontend)
```bash
cd "agri-ui"
npm install
npm run dev
```
> Running on: **http://localhost:3000**

---

### 2. API Gateway (Node.js)
```bash
cd "api-gateway"
npm install
npm run dev
```
> Running on: **http://localhost:5000**

---

### 3. Admin Service (.NET Backend)
```bash
cd "admin service"
dotnet run --launch-profile http
```
> Running on: **http://localhost:5001**

---

### 4. Farmer Service (.NET Backend)
```bash
cd "farmer service"
dotnet run --launch-profile http
```
> Running on: **http://localhost:5002**

---

### 5. Officer Service (.NET Backend)
```bash
cd "officer service"
dotnet run --launch-profile http
```
> Running on: **http://localhost:5003**

---

### 6. Supplier Service (.NET Backend)
```bash
cd "supplier service"
dotnet run --launch-profile http
```
> Running on: **http://localhost:5004**
