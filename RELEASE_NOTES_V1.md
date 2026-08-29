# Pars System v1.0 – Enterprise HR & Workflow

## امکانات این نسخه
- هسته یکپارچه .NET 8 با Clean Architecture
- مدیریت پرسنل و حضور و غیاب موجود
- ماژول مرخصی روزانه و ساعتی
- مانده و مصرف مرخصی سالانه
- ماژول مأموریت و اطلاعات مقصد/خودرو/جانشین
- Workflow دو مرحله‌ای مدیر مستقیم و منابع انسانی
- کارتابل تأیید و رد با ثبت توضیحات
- درخواست‌های من و تاریخچه مراحل
- داشبورد منابع انسانی با KPIهای عملیاتی
- Audit Log برای رخدادهای اصلی Workflow
- JWT / Roles (`User`, `Manager`, `HR`, `Admin`)
- رابط فارسی RTL و Responsive
- اسکریپت نصب SQL Server
- فایل اجرای Development در Windows

## پیش‌نیاز اجرا
1. .NET 8 SDK
2. SQL Server
3. تنظیم ConnectionString در `Pars.API/appsettings.json`
4. اجرای `scripts/001_enterprise_v1.sql`
5. اجرای `start-dev.bat` یا اجرای جداگانه پروژه‌های API و Web

## نکته
محیط تولید باید CORS محدود، HTTPS اجباری، Secret امن برای JWT، ذخیره Refresh Token و سیاست Backup دیتابیس داشته باشد.
