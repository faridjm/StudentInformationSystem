# Student Information System (SIS)

## Haqqında
Code Academy Final Project.
ASP.NET Core MVC və Web API ilə yazılmış tam funksional Tələbə İnformasiya Sistemi.

## Texniki Stack
- **Backend:** ASP.NET Core Web API (.NET 10)
- **Frontend:** ASP.NET Core MVC (.NET 10)
- **ORM:** Entity Framework Core 10
- **Database:** SQL Server Express
- **Auth:** ASP.NET Identity + JWT Bearer Token
- **Real-time:** SignalR
- **Mapping:** AutoMapper
- **Architecture:** Clean Architecture (BLL/DAL/API/MVC)

## Solution Strukturu
SIS.sln
├── SIS.API          → REST API endpoints, JWT Auth, SignalR Hub
├── SIS.MVC          → Razor Views, Controllers
├── SIS.BLL          → Business Logic, Services, DTOs, AutoMapper
├── SIS.DAL          → EF Core, Repository Pattern, UnitOfWork
├── SIS.Entities     → Domain Models, Enums
└── SIS.Common       → Helpers, Constants, Extensions

## Rollar
| Rol | Səlahiyyət |
|-----|-----------|
| SuperAdmin | Tam giriş — seed data ilə yaradılır |
| Admin | Sistem idarəetməsi |
| Moderator | Məzmun idarəetməsi |
| Teacher | Öz kursları, qiymətlər, davamiyyət |
| Student | Öz məlumatları, transcript |

## Tələblər (F1-F8)
| Kod | Təsvir |
|-----|--------|
| F1 | Tələbə profil idarəetməsi |
| F2 | Kurs kataloqu və müəllim təyinatı |
| F3 | Tələbə qeydiyyat sistemi |
| F4 | Qiymət girişi və transcript |
| F5 | Davamiyyət qeydiyyatı |
| F6 | GPA hesablama dashboard |
| F7 | Elan sistemi (institut + kurs) |
| F8 | Admin idarəetmə paneli |

## Kreativ Əlavələr
- GPA trend qrafiki (Chart.js)
- Davamiyyət heatmap (GitHub-style)
- QR kodlu PDF transcript
- Dark mode
- SignalR real-time bildirişlər
- Email bildirişləri
- Multilanguage dəstəyi (AZ/EN/RU)

## Default Giriş
| Email | Şifrə | Rol |
|-------|-------|-----|
| superadmin@sis.az | SuperAdmin@123 | SuperAdmin |

## appsettings.json Haqqında
`appsettings.json` faylı adətən `.gitignore`-a əlavə edilir, çünki
database connection string və JWT secret key kimi həssas məlumatlar saxlayır.
Lakin bu akademik layihədə müəllimin layihəni asanlıqla işə sala bilməsi
üçün qəsdən repo-ya daxil edilmişdir. Real production layihəsində
bu fayl mütləq `.gitignore`-a əlavə edilərdi.