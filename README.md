# DataCo E-Commerce Web & OLAP System

## 1. Giới thiệu đề tài

Đề tài xây dựng một hệ thống **website bán hàng thương mại điện tử** tích hợp **kho dữ liệu (Data Warehouse)** và **phân tích dữ liệu đa chiều (OLAP)** nhằm phục vụ cho việc phân tích, báo cáo và hỗ trợ ra quyết định.

Hệ thống bao gồm:

- Website bán hàng phát triển bằng **ASP.NET Core MVC**
- Quy trình **ETL** sử dụng **SQL Server Integration Services (SSIS)**
- Kho dữ liệu và khối phân tích **OLAP Cube** sử dụng **SQL Server Analysis Services (SSAS)**

---

## 2. Công nghệ và công cụ sử dụng

| STT | Nhóm công cụ | Tên phần mềm / dịch vụ | Mục đích |
| --- | ------------ | ---------------------- | -------- |
| 1 | IDE | Visual Studio 2022 | Phát triển, gỡ lỗi và quản lý dự án |
| 2 | Framework | ASP.NET Core (MVC) | Xây dựng website theo mô hình MVC |
| 3 | CSDL | SQL Server 2019 | Quản lý CSDL OLTP và Data Warehouse |
| 4 | ETL | SQL Server Integration Services (SSIS) | Tích hợp và nạp dữ liệu vào kho dữ liệu |
| 5 | OLAP | SQL Server Analysis Services (SSAS) | Xây dựng Cube phân tích dữ liệu |
| 6 | Web Server | Internet Information Services (IIS) | Triển khai website |
| 7 | Bảo mật & mạng | Cloudflare Tunnel (cloudflared) | Công bố website ra Internet an toàn |

---

## 3. Kiến trúc tổng thể hệ thống

Luồng xử lý dữ liệu:
```
OLTP Database -> Staging -> Data Warehouse -> SSAS Cube -> Web / BI
```
- Dữ liệu giao dịch được lưu trữ tại **CSDL OLTP**
- SSIS thực hiện ETL dữ liệu qua **Staging**
- Dữ liệu được tổ chức theo mô hình **Dimension – Fact** trong Data Warehouse
- SSAS xử lý và cung cấp Cube cho mục đích phân tích dữ liệu

---

## 4. Cấu trúc thư mục dự án
```
DataCo_Website_Project/
├── Database/ # Script SQL tạo OLTP & Data Warehouse
├── DataCo_Website/ # Website ASP.NET Core MVC
├── SSAS/ # Project OLAP (Cube)
├── SSIS/ # Project ETL
├── .gitignore
└── README.md
```
---

## 5. Cấu hình cơ sở dữ liệu và kho dữ liệu

### 5.1 OLTP Database
- Thực hiện **backup / restore** CSDL OLTP phục vụ hệ thống bán hàng  
- Ví dụ: `DataCo_Ecommerce`

### 5.2 Data Warehouse
- Thực thi script: 
```
Database/DataCo_DWH.sql
```
Script này thực hiện các chức năng sau:
- Tạo database Data Warehouse
- Tạo các bảng **Dimension** và **Fact**
- Thiết lập khóa chính và mối quan hệ
- Phục vụ cho phân tích dữ liệu đa chiều

### 5.3 Staging Database
- Được sử dụng làm vùng trung gian trong quá trình ETL
- Được khởi tạo trước khi thực thi các package SSIS

---

## 6. Cấu hình kết nối hệ thống

Các kết nối cần được cấu hình phù hợp với môi trường triển khai:

- **ASP.NET Web**: `appsettings.json`
- **SSIS Project**
- **SSAS Project**

### Lưu ý
File cấu hình mẫu được cung cấp: `appsettings.example.json`  
Người dùng cần sao chép và đổi tên thành `appsettings.json` trước khi chạy.

---

## 7. Triển khai OLAP (SSAS)

- Deploy SSAS Project để tạo **Cube dữ liệu**
- Cube được sử dụng cho:
  - Phân tích doanh thu
  - Phân tích theo thời gian, sản phẩm, khu vực
  - Phục vụ báo cáo và trực quan hóa dữ liệu

---

## 8. Triển khai ứng dụng Web

### Bước 1: Đóng gói ứng dụng
- Sử dụng tính năng **Publish** trong Visual Studio 2022
- Đóng gói dự án web ASP.NET

### Bước 2: Cấu hình IIS
- Tạo website trên IIS
- Trỏ đến thư mục publish
- Cấu hình cổng dịch vụ phù hợp

### Bước 3: Thiết lập Cloudflare Tunnel
- Sử dụng `cloudflared` để tạo tunnel
- Ánh xạ tên miền công khai đến ứng dụng chạy trên IIS
- Không cần mở cổng Router
- Tăng cường bảo mật và hỗ trợ HTTPS

---

## 9. Kết quả thực nghiệm và đánh giá

- Website bán hàng hoạt động ổn định
- Dữ liệu được tích hợp đầy đủ vào Data Warehouse
- Cube OLAP hỗ trợ phân tích dữ liệu đa chiều hiệu quả
- Hệ thống đáp ứng tốt yêu cầu phân tích và báo cáo dữ liệu

---

## 10. Kết luận

Hệ thống đã triển khai thành công mô hình tích hợp giữa:
- Website thương mại điện tử
- Kho dữ liệu
- Phân tích dữ liệu đa chiều (OLAP)

Giải pháp giúp nâng cao khả năng khai thác dữ liệu, hỗ trợ phân tích và ra quyết định cho hệ thống bán hàng.


