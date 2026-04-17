# System Zarządzania Urlopami (HR Management System)
Aplikacja mobilna/desktopowa zbudowana w technologii **.NET MAUI** z wykorzystaniem lokalnej bazy danych **SQLite**.

## 🚀 Funkcjonalności
- **Zarządzanie Pracownikami**: Dodawanie, edycja i przeglądanie listy pracowników.
- **Automatyczne Limity**: System automatycznie przypisuje limity urlopowe na podstawie wybranego typu zatrudnienia.
- **Inteligentny Kalkulator Urlopów**: Obliczanie dni roboczych z pominięciem weekendów oraz dni ustawowo wolnych od pracy (Holidays).
- **Historia Wniosków**: Pełny wgląd w historyczne i planowane urlopy każdego pracownika.
- **Dynamiczne Liczniki**: Podgląd pozostałych dni urlopu w czasie rzeczywistym.

## 🛠️ Stos Technologiczny
- **Framework**: .NET MAUI (C# / XAML)
- **Baza Danych**: SQLite (Biblioteka `sqlite-net-pcl`)
- **Architektura**: MVVM (Model-View-ViewModel)
- **Wstrzykiwanie Zależności**: Microsoft.Extensions.DependencyInjection

## 🏗️ Architektura Bazy Danych
Aplikacja oparta jest na relacyjnym modelu danych (ORM). Główne tabele to:
- `Employee`: Dane osobowe pracowników.
- `EmploymentType`: Słownik typów umów (UoP, B2B, itd.) wraz z limitami.
- `Leave`: Rejestr wniosków urlopowych (data startu, końca, status).
- `LeaveLimit`: Tabela przechowująca limity dni na dany rok dla pracownika.
- `LeaveType`: Rodzaje nieobecności (Wypoczynkowy, Chorobowy).
- `Holiday`: Tabela dni świątecznych używana do walidacji wniosków.

## 📁 Struktura Projektu
- **Models/**: Definicje tabel SQLite jako klasy C#.
- **ViewModels/**: Logika biznesowa stron i obsługa komend (ICommand).
- **Views/**: Warstwa prezentacji (XAML).
- **Services/**: `DatabaseService.cs` – serce aplikacji zarządzające połączeniem i obliczeniami.

## ⚙️ Logika Biznesowa (Kluczowe fragmenty)

### Obliczanie Dni Roboczych
System analizuje każdy dzień w zakresie dat wniosku i sprawdza, czy nie jest to sobota, niedziela lub święto zapisane w tabeli `Holiday`.

### Zarządzanie Stanem (Nawigacja)
Użyto mechanizmu **Shell Navigation** do przekazywania obiektów między stronami:
```csharp
await Shell.Current.GoToAsync("EmployeeDetailPage", new Dictionary<string, object> { { "Employee", selectedEmployee } });