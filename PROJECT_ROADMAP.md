# Pars System Product Roadmap

## فاز A — Consolidation
- یکپارچه‌سازی سورس Workflow/Request در Solution اصلی
- حذف duplicateهای Pages/shared با casing متفاوت
- استانداردسازی namespace و DI
- Health check و logging
- Migration strategy برای دیتابیس Legacy

## فاز B — HR Core
- ساختار سازمانی: سازمان/مدیریت/واحد/پست
- پرونده پرسنلی کامل
- قرارداد و وضعیت استخدام
- گروه کاری، شیفت و تقویم
- ضمائم و سوابق

## فاز C — Time & Attendance
- ورود دستگاه/فایل تردد
- محاسبه روزانه
- تأخیر، تعجیل، غیبت، اضافه‌کار
- اصلاح تردد با گردش تأیید
- اتصال به شیفت و تعطیلات

## فاز D — Leave
- انواع مرخصی و سهمیه
- درخواست ساعتی/روزانه
- کنترل تداخل
- جانشین
- گردش تأیید
- اثر خودکار روی Attendance

## فاز E — Mission
- مأموریت ساعتی/روزانه
- مقصد، وسیله، شرح، هزینه و ضمائم
- گردش تأیید
- اثر خودکار روی Attendance

## فاز F — Workflow / Cartable
- تعریف Workflow قابل تنظیم
- مراحل و قوانین تأیید
- Inbox / Outbox / History
- Delegation / Substitute
- SLA و Escalation
- Notification

## فاز G — Management Dashboard
- KPI پرسنلی
- حضور و غیاب
- مرخصی و مأموریت
- درخواست‌های معوق
- روندها و هشدارها
- فیلتر سازمانی و زمانی

## فاز H — Platform
- RBAC کامل
- Audit Trail
- System Settings
- Backup/Restore
- Import/Export Excel
- PDF/Print
- Persian calendar & RTL
- Theme / Font / Accessibility

## فاز I — Production
- Integration tests
- Security hardening
- Docker compose
- CI/CD
- Production configuration
- Deployment handbook
