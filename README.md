# AddinVeMong
<img width="1148" height="783" alt="image" src="https://github.com/user-attachments/assets/1307c7d5-c3bb-41e3-a232-8caf5bc6eab2" />



## Cấu trúc thư mục
Resources
= Styles: Chứa style cho textbox
- Localization: Chứa bản dịch
- Images: Icon cho button
Views: Chứa giao diện
ViewModels: Chứa logic vẽ thép
Models: Lưu dữ liệu

## Phiên bản sử dụng
- Revit 2025.0.2

## Cấu hình file .addin
```xml
<?xml version="1.0" encoding="utf-8"?>
<RevitAddIns>
  <AddIn Type="Application">
    <Name>AddinVeMong</Name>
    <Assembly>Path\AddinVeMong.dll</Assembly>
    <AddInId>67E41F9B-ECB4-43F3-84CA-694CF65154B5</AddInId>
    <FullClassName>AddinVeMong.App</FullClassName>
    <VendorId>com.yourname</VendorId>
    <VendorDescription>Phần mềm vẽ móng</VendorDescription>
  </AddIn>
</RevitAddIns>
```

**Thay Path bằng đường dẫn riêng trên máy**
