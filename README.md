# Garage Manager

FullStack system for garage management

## Project Description
Garage Manager is a garage management system that includes a C# (ASP.NET Core) API and a modern Angular 17 client with Angular Material. The system allows loading garages from a government source, adding them to the system, preventing duplicates, displaying data in a table, validation, and user feedback.

---

## Main Technologies
- **Backend:** ASP.NET Core 6, MongoDB
- **Frontend:** Angular 17 (standalone components), Angular Material
- **Services:** HttpClient, Observable
- **DevOps:** Git

---

## Installation & Running

### Backend (C#)
1. Open the `GarageApi` folder in Visual Studio/VS Code.
2. Make sure .NET 6 or higher is installed.
3. Set the MongoDB connection string in `appsettings.json`.
4. Run the server:
   ```bash
   dotnet run --project GarageApi/GarageApi.csproj
   ```
5. Test the API in Swagger at: `https://localhost:7034/swagger`

### Frontend (Angular)
1. Open the `GarageAngular` folder.
2. Install dependencies:
   ```bash
   npm install
   ```
3. Run the client:
   ```bash
   ng serve
   ```
4. Open your browser at `http://localhost:4200`

---

## Main Features
- Load garages from the government API (via the server)
- Select garages from a list (Multi-Select)
- Add only new garages (prevent duplicates)
- Display existing garages in a styled table
- Client-side validation and error/success feedback
- Modern, responsive, and clean design

---

## Project Structure
```
GarageManager/
│
├── GarageApi/                # Backend (C#)
│   ├── Controllers/
│   ├── Models/
│   ├── Services/
│   ├── appsettings.json
│   └── ...
│
└── GarageAngular/            # Frontend (Angular)
    ├── src/app/components/garages/
    ├── src/app/services/
    ├── src/app/app.html
    └── ...
```

---

## Contribution
Feel free to open Issues or Pull Requests for improvements, bug fixes, or new features.

---

## Credits
- Development: [rivka kirshtein]

---

## License
MIT
