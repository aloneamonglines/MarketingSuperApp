# Kế hoạch triển khai Identity & Access — MarketingSuperApp (MKS)

> Phiên bản: 1.0 · Ngày: 2026-11-04 · Trạng thái: **Đã chốt, sẵn sàng triển khai**
> Phạm vi: lập kế hoạch, chưa viết code. Kế hoạch bám sát quy ước, kiến trúc và cấu trúc hiện có của repo `MarketingSuperApp-main`.

---

## 1. Tóm tắt điều hành

Hệ thống là nền tảng **nhiều app, nhiều tenant** trên .NET 10 và PostgreSQL. Repo hiện mới là khung sườn: shared kernel (6 tầng) và `MKS.Gateway.API`. Chưa có service Identity, Tenant hay App; chưa có EF Core, xác thực hay reverse proxy.

Kế hoạch này xây **một service lõi "Identity & Access"** gồm ba module (Identity, Tenancy/App catalog, Authorization) dùng chung một database và một transaction. Các service nghiệp vụ marketing tách riêng và chỉ nhận **sự kiện tích hợp** từ lõi. Lý do không tách ba service ngay: tránh các vấn đề nhất quán phân tán (mời user vào tenant vừa bị xóa, tham chiếu mồ côi) khi chưa có số liệu cho thấy cần.

---

## 2. Các quyết định đã chốt

| # | Quyết định | Nguồn |
|---|---|---|
| D1 | Cơ sở dữ liệu: **PostgreSQL**, cô lập tenant bằng Row-Level Security | Người dùng |
| D2 | Seed sẵn **một tài khoản SuperAdmin** | Người dùng |
| D3 | Mỗi app có **một hoặc nhiều AppAdmin**, luôn còn ít nhất một | Người dùng |
| D4 | AppAdmin có **toàn quyền** permission/function của app mình và **không được sửa** định nghĩa permission/function | Người dùng |
| D5 | AppAdmin tạo tenant, tạo user trong tenant, tạo role, gán permission/function của app đó | Người dùng |
| D6 | Có **TenantAdmin**; mỗi tenant luôn còn ít nhất một để không phải nhờ AppAdmin gán lại | Người dùng |
| D7 | AppAdmin **tạo user trực tiếp** với mật khẩu mặc định **`123456`** | Người dùng |
| D8 | SuperAdmin **không xem dữ liệu tenant** mặc định; chỉ qua **break-glass** có lý do, thời hạn, audit | Người dùng |
| D9 | Định danh đăng nhập: **email / số điện thoại / username**, bắt buộc ít nhất một, giá trị trống được bù từ giá trị còn lại | Người dùng |
| D10 | `SendNotification` chỉ làm **skeleton**, xây thật sau | Người dùng |
| D11 | Kiến trúc: một service lõi Identity & Access (modular), các service nghiệp vụ tách riêng | Đề xuất, đã duyệt |
| D12 | Giao thức: dùng sản phẩm đã kiểm chứng (**OpenIddict**), không tự viết lõi OAuth/OIDC | Đề xuất, đã duyệt |
| D13 | Tất cả các điểm lệch ở mục 15 được chấp thuận với đề xuất mặc định | Người dùng |

---

## 3. Hiện trạng repo (đã kiểm chứng)

- `src/shared`: `MKS.Shared.Domain.Shared`, `Domain`, `Application.Contracts`, `Application`, `Infrastructure`, `Host`.
- `src/gateways/MKS.Gateway.API`: ASP.NET Core thuần, còn endpoint `weatherforecast` mẫu, `AllowedHosts: "*"`, chưa xác thực.
- Đã có: `BaseEntity` (Guid v7), `FullAuditEntity` (soft delete, creator/modifier/deleter/restorer), `IHasMasterStatusEntity` + `CMasterStatus`, `IHasAppEntity` và `IHasTenantEntity` (**đều nullable**), `IBaseProfile`, `IHasTenantDto`, `Address`/`Image` value object, localization JSON nhúng (en, vi), `SystemConstants.SystemUser`.
- Còn rỗng: `IAppRepository`, `ITenantRepository`, `IAppService`, `AppDto`, `UpdateAppDto`, `IHasDistributedEvent`, `IHasLocalEvent`.
- Chưa có: `.sln`, `Directory.Build.props`, project test, EF Core, xác thực, quy ước báo lỗi nghiệp vụ, quy ước đặt tên bảng.

---

## 4. Quy ước repo bắt buộc tuân thủ

| Hạng mục | Quy ước quan sát được | Áp dụng cho code mới |
|---|---|---|
| Đặt tên project | `MKS.<Phạm vi>.<Tầng>` gồm 6 tầng: Domain.Shared, Domain, Application.Contracts, Application, Infrastructure, Host | Service mới lặp lại đúng 6 tầng; web project là `.API` như Gateway |
| Hướng tham chiếu | Domain.Shared ← Domain, Contracts; Application → Contracts + Domain; Infrastructure → Domain + Contracts (không tham chiếu Application); Host → Infrastructure + Application | Giữ nguyên, không đảo chiều |
| Thư mục | Theo tính năng rồi theo loại: `Apps/Entities`, `Apps/Repositories`, `Apps/DTOs`, `Apps/Interfaces`, `MultiTenancy/...` | Mỗi tính năng có đủ thư mục tương ứng |
| Namespace | Block-scoped, khớp đường dẫn thư mục | Giữ |
| Entity | Kế thừa `BaseEntity` hoặc `FullAuditEntity` | Entity nghiệp vụ dùng `FullAuditEntity`; `AuditLog` chỉ thêm nên dùng `BaseEntity` |
| Interface đặc tính | `IHas…Entity` ở Domain, `IHas…Dto` ở Contracts | Giữ |
| Enum | Tiền tố `C`, đặt ở `Domain.Shared/Enums` | `CMembershipStatus`, `CIdentifierType`, `CEntityScope`, `CNotificationChannel`... |
| DTO / service | `XDto`, `CreateXDto`, `UpdateXDto`; `IXService : IApplicationService` | Giữ |
| Repository | Interface ở `Domain/<Tính năng>/Repositories` | Giữ; bản cài đặt ở Infrastructure |
| Localization | Lớp resource có `[LocalizationResource]`, file `en.json` + `vi.json` nhúng bằng `LogicalName`, khóa dạng `Nhóm.Nhóm.Tên` | Mọi thông báo và nhãn có khóa ở cả hai ngôn ngữ |
| Host | Tiện ích `AddShared…` / `UseShared…` | Thêm `AddSharedNotifications`, `AddSharedPersistence`... |
| Controller | `[ApiController]`, lớp `sealed`, primary constructor, route chữ thường `api/...` | Giữ; mỗi nhóm API có file `RESTClient/*.http` |
| Tài liệu mã | XML summary trên thành viên công khai | Tiếng Anh cho code mới |

---

## 5. Cấu trúc đích

```
src/
├─ shared/                              (giữ nguyên 6 project, bổ sung thư mục tính năng)
├─ gateways/
│  └─ MKS.Gateway.API/
└─ services/
   └─ identity/
      ├─ MKS.Identity.Domain.Shared
      ├─ MKS.Identity.Domain
      ├─ MKS.Identity.Application.Contracts
      ├─ MKS.Identity.Application
      ├─ MKS.Identity.Infrastructure    (EF Core, Npgsql, repository, migration, seed)
      └─ MKS.Identity.API               (web project, tham chiếu Shared.Host)
tests/                                   (project test, xem mục 14)
```

`IAppRepository` và `ITenantRepository` hiện ở Shared. Giữ ở đó như hợp đồng dùng chung và cài đặt trong `MKS.Identity.Infrastructure`. Phần `Apps/` và `MultiTenancy/` của Identity dùng lại đúng tên thư mục này.

---

## 6. Thay đổi trong shared kernel (làm trước)

| Việc | Vị trí | Ghi chú |
|---|---|---|
| `IHasTenantEntity.TenantId` và `IHasAppEntity.AppId` đổi sang `Guid` **không null** | `Shared.Domain` | Hiện chưa nơi nào dùng nên không gây vỡ |
| Thêm `IMayHaveTenantEntity`, `IMayHaveAppEntity` (nullable) | `Shared.Domain` | Cho dữ liệu chung hoặc cấp app |
| Thêm `[EntityScope]` và `CEntityScope` (Global, App, Tenant) | `Domain.Shared/Attributes`, `Domain.Shared/Enums` | Mỗi entity bắt buộc khai báo; test kiến trúc kiểm tra khớp với marker |
| Thêm `ICurrentContext`: UserId, SessionId, TenantId, AppId, ImpersonatorId, BreakGlassGrantId | `Application.Contracts/Common/Interfaces` | Nguồn duy nhất là claim JWT đã verify |
| Thêm `IIntegrationEvent`: EventId, TenantId, OccurredAt, SchemaVersion | `Domain.Shared/Common/Events` | Bổ sung cho marker `IHasDistributedEvent` |
| Hạ tầng persistence dùng chung: interceptor audit và soft delete, interceptor gán `TenantId`, interceptor `set_config`, outbox/inbox | `Shared.Infrastructure/Persistence` | Theo mẫu thư mục `Localization/` |
| Skeleton thông báo: `INotificationSender`, `NotificationMessage`, `CNotificationChannel`, `NullNotificationSender` | `Application.Contracts/Notifications`, `Infrastructure/Notifications` | Chỉ ghi log, không gửi; không đưa dữ liệu nhạy cảm vào log |
| `AddSharedNotifications`, `AddSharedPersistence` | `Shared.Host` | Theo mẫu `AddSharedLocalization` |
| `CreateAppDto` bỏ `abstract`; điền `AppDto` và `UpdateAppDto`; `IAppService` có hợp đồng | `Application.Contracts/Apps` | Đổi hợp đồng đã có, đã được duyệt |
| Không đổi `IBaseProfile` | `Application.Contracts/Common/Interfaces` | DTO user lấy email/số điện thoại chính từ bảng định danh |

---

## 7. Mô hình miền

### 7.1 Phân cấp quản trị

| Cấp | Là ai | Được làm | Không được làm |
|---|---|---|---|
| Platform | SuperAdmin (seed) | Tạo app, tạo AppAdmin, tạo role cấp nền tảng, quản trị toàn hệ thống | Đọc dữ liệu nghiệp vụ tenant (chỉ qua break-glass) |
| App | AppAdmin (nhiều người mỗi app) | Tạo tenant, user, role; gán permission/function của app mình | Sửa định nghĩa permission/function; chạm sang app khác |
| Tenant | TenantAdmin | Tạo/mời user, gán role, khóa/mở khóa thành viên **trong tenant của mình** | Vượt quá permission mình có; chạm tenant khác |
| Tenant | User thường | Thao tác trong phạm vi role được gán | Vượt quá permission của role |

### 7.2 Thực thể

| Entity | Thư mục (trong `MKS.Identity.Domain`) | Base / marker | Phạm vi |
|---|---|---|---|
| `User` | `Users/Entities` | FullAudit | Global |
| `UserIdentifier` (Email, Phone, Username) | `Users/Entities` | FullAudit | Global |
| `UserCredential` (hash, `MustChangePassword`, số lần sai, khóa tạm) | `Users/Entities` | FullAudit | Global |
| `App` | `Apps/Entities` | FullAudit + `IHasMasterStatusEntity` | Global |
| `AppAdminAssignment` | `Apps/Entities` | FullAudit + `IHasAppEntity` | App |
| `Tenant` | `MultiTenancy/Entities` | FullAudit + `IHasMasterStatusEntity` + `IHasAppEntity` | App |
| `TenantMembership` | `MultiTenancy/Entities` | FullAudit + `IHasTenantEntity` | Tenant |
| `TenantAdminAssignment` | `MultiTenancy/Entities` | FullAudit + `IHasTenantEntity` | Tenant |
| `AppFunction`, `AppPermission` | `Authorization/Entities` | FullAudit + `IHasAppEntity` | App (chỉ đọc với người dùng; đồng bộ từ code) |
| `Role` | `Authorization/Entities` | FullAudit + `IHasMasterStatusEntity` + `IHasAppEntity` + `IMayHaveTenantEntity` | App hoặc Tenant |
| `RoleFunction`, `RolePermission` | `Authorization/Entities` | FullAudit + `IHasAppEntity` | App |
| `RoleAssignment` | `Authorization/Entities` | FullAudit + `IHasTenantEntity` | Tenant |
| `PlatformRoleAssignment` (chứa SuperAdmin) | `Platform/Entities` | FullAudit | Global |
| `BreakGlassGrant` | `Platform/Entities` | BaseEntity | Global |
| `AuditLog` | `Auditing/Entities` | BaseEntity (chỉ thêm, không sửa) | Global |

### 7.3 Enum (`MKS.Identity.Domain.Shared/Enums`)

`CIdentifierType` (Email, Phone, Username), `CMembershipStatus` (Active, Suspended, Removed; giá trị `0` bị từ chối), `CBreakGlassStatus` (Active, Expired, Revoked). App, Tenant, Role dùng lại `CMasterStatus` có sẵn và từ chối giá trị `None` khi tạo.

### 7.4 Điểm cần lưu ý

- **SuperAdmin là tài khoản thật** có Guid riêng, khác với `SystemUser.Id` (`…0001`) vốn là tác nhân tự động.
- **Membership** gắn user với tenant. Tenant thuộc đúng một app, nên membership chỉ có hiệu lực trong app đó.
- **Role** có `AppId` bắt buộc. `TenantId` rỗng nghĩa là role dùng chung cả app (chỉ AppAdmin sửa); có giá trị nghĩa là role riêng tenant.
- **Function** là nhóm permission theo tính năng, dùng để gán nhanh và ẩn/hiện menu. **Permission** là quyền nhỏ nhất, dạng `app:resource:action`.
- Catalog permission/function **do code của app khai báo** và đồng bộ vào database khi khởi động. Giao diện không có chức năng thêm, sửa, xóa catalog. Permission không còn trong code được đánh dấu `Deprecated`, không xóa cứng.

---

## 8. Luật nghiệp vụ

### 8.1 Định danh đăng nhập

- Khi tạo user, bắt buộc có **ít nhất một** trong email, số điện thoại, username.
- Username trống thì tự điền từ email, nếu không có thì từ số điện thoại; giá trị tự điền đánh dấu `IsDerived = true`.
- Người dùng đăng nhập bằng bất kỳ định danh nào. Mọi giá trị được chuẩn hóa (chữ thường, bỏ khoảng trắng, số điện thoại về dạng E.164) và **unique toàn hệ thống**.
- Username do người dùng tự đặt không được có dạng email hoặc số điện thoại của người khác, để lúc đăng nhập không mơ hồ.
- Unique index có điều kiện `WHERE NOT "IsDeleted"` vì repo dùng soft delete.

### 8.2 Tạo user và mật khẩu mặc định

- AppAdmin hoặc TenantAdmin tạo user vào tenant. Nếu định danh đã tồn tại thì **chỉ thêm membership**: không đổi mật khẩu, không tiết lộ tài khoản đã có.
- Tài khoản mới có mật khẩu mặc định **`123456`** lấy từ cấu hình `Identity:DefaultPassword`, vẫn băm như mật khẩu thường.
- `MustChangePassword = true`: lần đầu đăng nhập chỉ gọi được API đổi mật khẩu, mọi API khác trả 403.
- Chính sách mật khẩu áp dụng cho mật khẩu **mới**, không áp dụng cho giá trị mặc định.
- Khóa tạm tài khoản khi nhập sai nhiều lần; giới hạn tốc độ theo tài khoản.
- Rủi ro còn lại (đã ghi nhận): ai biết định danh của user mới đều có thể đăng nhập trước họ bằng mật khẩu mặc định.

### 8.3 Bất biến "luôn còn ít nhất một"

| Đối tượng | Luật |
|---|---|
| SuperAdmin | Luôn còn ít nhất một trên nền tảng |
| AppAdmin | Luôn còn ít nhất một mỗi app |
| TenantAdmin | Luôn còn ít nhất một mỗi tenant; tạo tenant bắt buộc kèm một TenantAdmin ban đầu |

- Cài đặt ở **domain service** (thư mục `Services` trong từng tính năng), kiểm tra trong **cùng transaction** kèm khóa dòng để hai thao tác đồng thời không vượt qua nhau.
- Thao tác bị chặn: gỡ role, xóa user, khóa user, xóa membership, gỡ assignment làm số admin hoạt động về 0.
- Lớp phòng thủ cuối: constraint trigger ở PostgreSQL.
- **Đường cứu hộ:** AppAdmin gán lại TenantAdmin cho tenant (ví dụ khi người duy nhất nghỉ việc); thao tác được audit.

### 8.4 Chống leo thang quyền

- Người gán role chỉ được gán permission mà chính họ đang có. AppAdmin có tất cả nên không bị giới hạn.
- Không thể gán permission của app A cho role của app B: khóa ngoại tổng hợp `(AppId, Code)` ở `RolePermission` và `RoleFunction`.
- `RoleAssignment` chỉ hợp lệ khi membership thuộc tenant của đúng app của role.

### 8.5 Break-glass (SuperAdmin truy cập dữ liệu tenant)

- Mặc định SuperAdmin **không** có quyền đọc dữ liệu tenant.
- Truy cập chỉ qua `BreakGlassGrant`: lý do bắt buộc (tối thiểu 20 ký tự), tenant mục tiêu, thời hạn tối đa **60 phút**, mặc định **chỉ đọc**. Các giá trị này chỉnh được qua cấu hình.
- Token break-glass mang claim `bg` và hết hạn cùng grant. Mọi truy cập ghi audit kèm `BreakGlassGrantId`.
- Grant có thể thu hồi sớm.

### 8.6 Seed SuperAdmin

- Seeder ở Infrastructure tạo SuperAdmin khi khởi động nếu chưa có.
- Email và mật khẩu đọc từ cấu hình hoặc secret; **không có giá trị mặc định trong code**; thiếu cấu hình thì seeder báo lỗi rõ ràng.
- `MustChangePassword = true`.

---

## 9. Thuật toán kiểm tra quyền

Kiểm tra lần lượt, dừng ở bước đầu tiên cho kết quả:

1. **Quyền nền tảng:** `PlatformRoleAssignment` còn hiệu lực thì cho phép các permission cấp nền tảng. Không mở rộng sang dữ liệu tenant.
2. **AppAdmin:** user có `AppAdminAssignment` của đúng app thì cho phép **mọi** permission của app. Đây là quy tắc tính toán, không lưu từng dòng, nên permission mới của app tự có hiệu lực.
3. **TenantAdmin:** cho phép các permission quản trị tenant, chỉ trong tenant đó.
4. **Role:** gộp permission từ `RoleFunction` và `RolePermission` của các `RoleAssignment` còn hiệu lực trong membership.

Kết quả được cache theo `MembershipVersion`. Đổi role hoặc gỡ membership thì tăng version và phát sự kiện để xóa cache, nên hiệu lực gần như tức thì.

---

## 10. Cô lập dữ liệu trên PostgreSQL

1. Mọi bảng thuộc tenant có `TenantId NOT NULL` và bật `ROW LEVEL SECURITY`.
2. Ở mỗi transaction, ứng dụng gọi `set_config` với tham số cục bộ theo transaction; policy so sánh `TenantId` với giá trị đó. Đặt cục bộ theo transaction để an toàn khi dùng PgBouncer.
3. Ứng dụng kết nối bằng một role Postgres **không sở hữu bảng** và **không có `BYPASSRLS`**. Migration chạy bằng role riêng.
4. `TenantId` chỉ được gán bởi interceptor khi `SaveChanges`, không có setter công khai cho code nghiệp vụ.
5. EF Core global query filter làm lớp thứ hai; RLS là lớp cuối.
6. Khóa ngoại tổng hợp `(TenantId, Id)` giữa các bảng tenant để không thể tham chiếu chéo tenant.
7. Tenant lớn hoặc có yêu cầu tuân thủ có thể chuyển sang schema hoặc database riêng sau này; thiết kế không chặn hướng này.

Quy ước đặt tên (đề xuất đã duyệt): một schema `identity` cho service; cột theo tên thuộc tính.

---

## 11. Gateway và token

**Gateway (`MKS.Gateway.API`)**
- Xóa endpoint `weatherforecast` mẫu.
- Thêm xác thực JWT, định tuyến bằng YARP.
- Xóa mọi header `X-Tenant-*` do client gửi; ngữ cảnh chỉ lấy từ token.
- Giới hạn tốc độ theo tenant, gắn correlation id, HSTS, CORS theo danh sách cho phép.
- Đổi `AllowedHosts` khỏi `*`.

**Token**
- Access token sống 5–10 phút, mang `sub`, `sid`, `tid`, `aid` (và `bg` khi break-glass). **Không** chứa danh sách permission.
- Đổi tenant thì đổi token (token exchange).
- Refresh token xoay vòng, phát hiện dùng lại; thu hồi theo session và theo thiết bị.
- Permission được tính ở server theo mục 9.

**Impersonation:** audit tách `ActingAsUserId` và `ImpersonatorId`; tối đa một cấp.

---

## 12. Skeleton thông báo (`SendNotification`)

- Hợp đồng ở `Shared.Application.Contracts/Notifications`: gửi theo `TemplateCode`, người nhận, kênh (Email, Sms, InApp), dữ liệu template.
- Bản mặc định `NullNotificationSender` ở `Shared.Infrastructure/Notifications`: chỉ ghi log việc bỏ qua gửi.
- Đăng ký bằng `AddSharedNotifications` ở `Shared.Host`.
- Luồng tạo user phát sự kiện tích hợp (ví dụ `UserCreated`) để sau này sender thật bám vào, không cần sửa luồng tạo user.
- Không đưa mật khẩu hoặc dữ liệu nhạy cảm vào log hay `Data` của bản rỗng.

---

## 13. Lộ trình triển khai (work package)

| WP | Nội dung | Tiêu chí hoàn thành |
|---|---|---|
| 0 | Chốt các điểm ở mục 15 | **Đã xong** (mục D13) |
| 1 | Sửa shared kernel (mục 6) | Build sạch; test kiến trúc xác nhận mọi entity khai báo scope khớp marker |
| 2 | Dựng 6 project Identity, localization en/vi, `.sln`, `Directory.Build.props` | Build chạy được, tham chiếu đúng hướng, có test kiểm tra hướng tham chiếu |
| 3 | Domain: entity, enum, repository interface, domain service, bất biến | Test đơn vị cho toàn bộ luật ở mục 8 |
| 4 | Infrastructure: EF Core, cấu hình entity, migration, RLS, trigger, seed | Test chứng minh truy vấn không có tenant context trả về rỗng |
| 5 | Application: DTO, service, mapping, tạo user, tạo tenant, quản lý role | Test luồng AppAdmin tạo tenant, role, user và gán quyền |
| 6 | API: controller, `RESTClient`, OpenIddict, đổi mật khẩu bắt buộc, đổi tenant | Đăng nhập, đổi mật khẩu lần đầu, đổi tenant chạy đầu cuối |
| 7 | Gateway: JWT, YARP, chặn header, giới hạn tốc độ | Không token trả 401; header giả mạo bị bỏ |
| 8 | Break-glass, audit, đồng bộ catalog permission, cache theo version | Grant hết hạn bị từ chối; audit đủ trường; gỡ role có hiệu lực ngay |
| 9 | Test bảo mật tổng hợp | Toàn bộ test ở mục 14 đạt trong CI |

Thứ tự phụ thuộc: 1 → 2 → 3 → 4 → 5 → 6 → 7; WP 8 sau WP 6; WP 9 chạy xuyên suốt và chốt cuối cùng.

---

## 14. Kế hoạch kiểm thử

| Nhóm | Nội dung kiểm tra |
|---|---|
| Kiến trúc | Hướng tham chiếu giữa các project; mọi entity có `[EntityScope]` khớp marker; không entity tenant nào có `TenantId` nullable |
| Bất biến | Không thể gỡ SuperAdmin, AppAdmin, TenantAdmin cuối cùng; **hai thao tác đồng thời** cùng gỡ hai admin cuối không thể cùng thành công |
| Cô lập | Đọc, sửa, xóa chéo tenant đều thất bại ở cả tầng EF và tầng RLS; kết nối không đặt `set_config` trả về rỗng |
| Phân quyền | Ma trận SuperAdmin / AppAdmin / TenantAdmin / user thường × quyền; AppAdmin không sửa được catalog; không gán permission chéo app; không leo thang quyền |
| Định danh | Chuẩn hóa và unique; tự điền username; tạo user với định danh đã có chỉ thêm membership và không đổi mật khẩu |
| Mật khẩu | `MustChangePassword` chặn mọi API trừ đổi mật khẩu; khóa tạm khi nhập sai; mật khẩu mới phải qua chính sách |
| Break-glass | Cần lý do, hết hạn đúng giờ, bị thu hồi được, mọi truy cập có audit |
| Token | Hết hạn, xoay vòng refresh, phát hiện dùng lại, đổi tenant tạo token mới |
| Seed | Thiếu cấu hình SuperAdmin thì báo lỗi rõ; chạy lại không tạo trùng |

---

## 15. Các điểm lệch khỏi repo hiện tại (đã được chấp thuận)

Repo chưa có các thứ sau; đã duyệt theo đề xuất:

1. Thư mục `src/services/identity/` làm vị trí cho service.
2. File `.sln`, `Directory.Build.props` và các project test.
3. Gói mới: EF Core, Npgsql, OpenIddict, YARP và thư viện test.
4. Cách báo lỗi nghiệp vụ: một `DomainException` mang **khóa localization**, đặt ở `Domain.Shared`.
5. Đặt tên bảng và schema PostgreSQL: schema `identity`, cột theo tên thuộc tính.
6. Đổi `CreateAppDto` từ `abstract`; đổi `TenantId`/`AppId` sang không null.

---

## 16. Rủi ro và biện pháp

| Rủi ro | Mức | Biện pháp |
|---|---|---|
| Mật khẩu mặc định `123456` bị đoán | Cao | `MustChangePassword`, khóa tạm, giới hạn tốc độ; rủi ro còn lại đã được chủ dự án ghi nhận |
| Rò rỉ dữ liệu giữa tenant | Cao | `TenantId` không null, RLS, query filter, khóa ngoại tổng hợp, test cô lập |
| Tranh chấp khi gỡ admin cuối | Trung bình | Transaction + khóa dòng + constraint trigger + test đồng thời |
| Quyền cũ còn hiệu lực sau khi thu hồi | Trung bình | Token ngắn hạn, cache theo `MembershipVersion`, sự kiện làm mới |
| Nhầm định danh lúc đăng nhập | Trung bình | Chuẩn hóa, unique toàn hệ thống, hạn chế username trùng dạng email/số điện thoại |
| `SuperAdmin` bị chiếm | Cao | Mật khẩu từ secret, đổi bắt buộc, thêm MFA ở giai đoạn sau, không có quyền đọc dữ liệu tenant mặc định |
| Phụ thuộc OpenIddict | Thấp | Là thư viện kiểm chứng; giữ ranh giới qua lớp `Application.Contracts` |

---

## 17. Ngoài phạm vi (làm sau)

SSO theo tenant (OIDC/SAML), SCIM, MFA và passkey, gửi thông báo thật, tier cô lập schema/database riêng cho tenant lớn, xóa và ẩn danh dữ liệu theo GDPR, tách Identity/Tenant/App thành service riêng nếu số liệu vận hành cho thấy cần.

---

## 18. Nguồn

Toàn bộ nội dung dựa trên việc đọc 54 file của repo `MarketingSuperApp-main` và các yêu cầu do chủ dự án cung cấp trong cuộc trao đổi.
