# Pars System v1.0 – Enterprise HR & Workflow

## معماری
- `Pars.Domain`: موجودیت‌های پرسنلی، حضور و غیاب و موجودیت‌های Enterprise Workflow.
- `Pars.Application`: قراردادها و DTOهای سرویس‌ها.
- `Pars.Infrastructure`: EF Core، SQL Server، JWT و پیاده‌سازی سرویس‌ها.
- `Pars.API`: REST API با JWT Authentication.
- `Pars.Web`: Blazor WebAssembly، RTL و رابط فارسی.

## چرخه درخواست
1. کاربر درخواست مرخصی یا مأموریت را ایجاد می‌کند (`Draft`).
2. درخواست Submit می‌شود و وارد وضعیت `Pending` می‌شود.
3. مرحله ۱ به Role=`Manager` ارسال می‌شود.
4. پس از تأیید مدیر، مرحله ۲ به Role=`HR` می‌رود.
5. تأیید HR درخواست را `Approved` می‌کند؛ رد در هر مرحله وضعیت را `Rejected` می‌کند.
6. در مرخصی تأییدشده، مصرف در `hr.LeaveBalances` ثبت می‌شود.
7. تمام Create/Submit/Approve/Reject در `audit.AuditLogs` ثبت می‌شود.

## امنیت
- API جدید Enterprise با `[Authorize]` محافظت می‌شود.
- تشخیص کاربر از JWT `NameIdentifier` انجام می‌شود.
- تصمیم‌گیری Approval بر اساس Role جاری (`Manager`, `HR`, `Admin`) کنترل می‌شود.

## دیتابیس
به دلیل وجود دیتابیس Legacy و سیاست جلوگیری از تغییر خودکار Schema در Startup، ایجاد جداول جدید با `scripts/001_enterprise_v1.sql` انجام می‌شود.
