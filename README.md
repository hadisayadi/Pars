# Pars System v1.0 – Enterprise HR & Workflow

سامانه یکپارچه منابع انسانی، حضور و غیاب و گردش کار سازمانی بر پایه .NET 8.

## ماژول‌های فعال
- احراز هویت JWT و نقش‌های سازمانی
- پرونده پرسنلی
- حضور و غیاب و کارکرد روزانه
- مرخصی روزانه/ساعتی و مانده مرخصی
- مأموریت اداری
- Workflow دو مرحله‌ای Manager → HR
- کارتابل تأیید/رد
- درخواست‌های من و تاریخچه گردش کار
- داشبورد منابع انسانی
- Audit Log

## ساختار Solution
- `Pars.Domain` — Domain Entities
- `Pars.Application` — Contracts/DTOs
- `Pars.Infrastructure` — EF Core, SQL Server, Services, Auth
- `Pars.API` — ASP.NET Core REST API
- `Pars.Web` — Blazor WebAssembly RTL UI
- `Pars.Tests` — Tests
- `scripts` — Database installation scripts
- `docs` — Architecture notes

## نصب
1. .NET 8 SDK و SQL Server را نصب کنید.
2. Connection String را در `Pars.API/appsettings.json` تنظیم کنید.
3. روی دیتابیس مقصد `scripts/001_enterprise_v1.sql` را اجرا کنید.
4. `start-dev.bat` را اجرا کنید، یا:
   - `dotnet run --project Pars.API/Pars.API.csproj`
   - `dotnet run --project Pars.Web/Pars.Web.csproj`
5. آدرس API در `Pars.Web/wwwroot/appsettings.json` قابل تنظیم است.

## نقش‌ها
`User`, `Manager`, `HR`, `Admin` در اسکریپت دیتابیس ساخته می‌شوند. برای گردش تأیید، کاربر تأییدکننده باید Role مناسب داشته باشد.

## امنیت Production
- کلید JWT را از Secret Store/Environment دریافت کنید.
- CORS را به دامنه‌های واقعی محدود کنید.
- ثبت‌نام عمومی (`api/auth/register`) را پس از ایجاد حساب‌های اولیه محدود کنید.
- HTTPS، Backup و Audit retention فعال شود.

برای جزئیات بیشتر: `docs/ARCHITECTURE_V1.md` و `RELEASE_NOTES_V1.md`.
