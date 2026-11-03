using System.Text.RegularExpressions;

namespace MKS.Shared.Domain.Shared.Common.ValueObjects
{
    /// <summary>
    /// Đại diện cho một địa chỉ địa lý trong hệ thống.
    /// 
    /// Address là Value Object bất biến, không có định danh riêng
    /// và được so sánh dựa trên giá trị của các thuộc tính.
    /// 
    /// Value Object này có thể được sử dụng cho địa chỉ cá nhân,
    /// địa chỉ doanh nghiệp, địa chỉ thanh toán, giao hàng
    /// hoặc địa chỉ kho hàng.
    /// 
    /// Address không chứa thông tin người nhận, số điện thoại,
    /// mã số thuế hoặc các thuộc tính phụ thuộc vào ngữ cảnh sử dụng.
    /// </summary>
    public sealed record Address
    {
        private const int MaxAddressLineLength = 255;
        private const int MaxDependentLocalityLength = 100;
        private const int MaxLocalityLength = 100;
        private const int MaxAdministrativeAreaLength = 100;
        private const int MaxPostalCodeLength = 20;
        private const int MaxCountryCodeLength = 2;

        private static readonly Regex CountryCodeRegex = new(
            @"^[A-Z]{2}$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>
        /// Địa chỉ chi tiết cấp đường phố hoặc số nhà.
        /// 
        /// Ví dụ:
        /// "123 Nguyễn Huệ"
        /// "Tầng 5, Tòa nhà ABC"
        /// 
        /// Đây là thành phần địa chỉ chính và không được để trống.
        /// </summary>
        public string AddressLine1 { get; private init; }

        /// <summary>
        /// Thông tin địa chỉ bổ sung.
        /// 
        /// Ví dụ:
        /// "Căn hộ 1205"
        /// "Tòa nhà B, Khu đô thị XYZ"
        /// 
        /// Thuộc tính này không bắt buộc.
        /// </summary>
        public string? AddressLine2 { get; private init; }

        /// <summary>
        /// Đơn vị địa lý nhỏ hơn thành phố hoặc tỉnh,
        /// thường được sử dụng để biểu diễn phường, xã,
        /// quận, huyện hoặc khu vực tương đương.
        /// 
        /// Ví dụ:
        /// "Phường Bến Nghé"
        /// "Quận 1"
        /// "Ward 1"
        /// 
        /// Ý nghĩa cụ thể phụ thuộc vào quy tắc địa chỉ
        /// của từng quốc gia.
        /// </summary>
        public string? DependentLocality { get; private init; }

        /// <summary>
        /// Thành phố, thị xã hoặc đơn vị địa lý tương đương.
        /// 
        /// Ví dụ:
        /// "Ho Chi Minh City"
        /// "Hanoi"
        /// "Da Nang"
        /// 
        /// Đây là thành phần địa chỉ bắt buộc.
        /// </summary>
        public string Locality { get; private init; }

        /// <summary>
        /// Tỉnh, bang, vùng hoặc đơn vị hành chính cấp cao hơn.
        /// 
        /// Ví dụ:
        /// "Ho Chi Minh"
        /// "California"
        /// "Ontario"
        /// 
        /// Ý nghĩa cụ thể phụ thuộc vào cấu trúc hành chính
        /// của quốc gia tương ứng.
        /// </summary>
        public string? AdministrativeArea { get; private init; }

        /// <summary>
        /// Mã bưu chính của địa chỉ.
        /// 
        /// Ví dụ:
        /// "700000"
        /// "100000"
        /// "90210"
        /// 
        /// Được lưu dưới dạng string để hỗ trợ số 0 ở đầu
        /// và các định dạng mã bưu chính quốc tế.
        /// Thuộc tính này không bắt buộc.
        /// </summary>
        public string? PostalCode { get; private init; }

        /// <summary>
        /// Mã quốc gia theo tiêu chuẩn ISO 3166-1 alpha-2.
        /// 
        /// Ví dụ:
        /// "VN"
        /// "US"
        /// "JP"
        /// 
        /// Giá trị được chuẩn hóa thành chữ in hoa.
        /// Đây là thành phần địa chỉ bắt buộc.
        /// </summary>
        public string CountryCode { get; private init; }

        /// <summary>
        /// Khởi tạo một Address Value Object.
        /// 
        /// Constructor private nhằm đảm bảo mọi đối tượng Address
        /// đều được tạo thông qua factory method có validation.
        /// </summary>
        private Address(
            string addressLine1,
            string? addressLine2,
            string? dependentLocality,
            string locality,
            string? administrativeArea,
            string? postalCode,
            string countryCode)
        {
            AddressLine1 = addressLine1;
            AddressLine2 = addressLine2;
            DependentLocality = dependentLocality;
            Locality = locality;
            AdministrativeArea = administrativeArea;
            PostalCode = postalCode;
            CountryCode = countryCode;
        }

        /// <summary>
        /// Tạo một Address Value Object với đầy đủ thông tin địa chỉ.
        /// 
        /// Đây là factory method chính được sử dụng khi tạo địa chỉ
        /// từ dữ liệu đã được xác định trong Domain.
        /// </summary>
        /// <param name="addressLine1">
        /// Địa chỉ chi tiết cấp đường phố hoặc số nhà.
        /// </param>
        /// <param name="locality">
        /// Thành phố hoặc đơn vị địa lý tương đương.
        /// </param>
        /// <param name="countryCode">
        /// Mã quốc gia theo ISO 3166-1 alpha-2.
        /// </param>
        /// <param name="addressLine2">
        /// Thông tin địa chỉ bổ sung, nếu có.
        /// </param>
        /// <param name="dependentLocality">
        /// Phường, xã, quận, huyện hoặc khu vực tương đương.
        /// </param>
        /// <param name="administrativeArea">
        /// Tỉnh, bang hoặc vùng hành chính, nếu có.
        /// </param>
        /// <param name="postalCode">
        /// Mã bưu chính, nếu có.
        /// </param>
        /// <returns>Một Address Value Object hợp lệ.</returns>
        /// <exception cref="ArgumentException">
        /// Ném ra khi một trong các thành phần địa chỉ không hợp lệ.
        /// </exception>
        public static Address Create(
            string addressLine1,
            string locality,
            string countryCode,
            string? addressLine2 = null,
            string? dependentLocality = null,
            string? administrativeArea = null,
            string? postalCode = null)
        {
            ValidateRequiredField(
                addressLine1,
                nameof(addressLine1),
                MaxAddressLineLength);

            ValidateRequiredField(
                locality,
                nameof(locality),
                MaxLocalityLength);

            ValidateCountryCode(countryCode);

            ValidateOptionalField(
                addressLine2,
                nameof(addressLine2),
                MaxAddressLineLength);

            ValidateOptionalField(
                dependentLocality,
                nameof(dependentLocality),
                MaxDependentLocalityLength);

            ValidateOptionalField(
                administrativeArea,
                nameof(administrativeArea),
                MaxAdministrativeAreaLength);

            ValidateOptionalField(
                postalCode,
                nameof(postalCode),
                MaxPostalCodeLength);

            return new Address(
                NormalizeRequiredValue(addressLine1),
                NormalizeOptionalValue(addressLine2),
                NormalizeOptionalValue(dependentLocality),
                NormalizeRequiredValue(locality),
                NormalizeOptionalValue(administrativeArea),
                NormalizeOptionalValue(postalCode),
                NormalizeRequiredValue(countryCode).ToUpperInvariant());
        }

        /// <summary>
        /// Tạo một Address Value Object dành cho địa chỉ tại Việt Nam.
        /// 
        /// Factory method này giúp việc tạo địa chỉ Việt Nam
        /// trở nên thuận tiện hơn bằng cách tự động sử dụng mã quốc gia VN.
        /// 
        /// Các thành phần hành chính vẫn được truyền vào theo
        /// cấu trúc địa chỉ mà hệ thống đang sử dụng.
        /// </summary>
        /// <param name="addressLine1">
        /// Địa chỉ chi tiết cấp đường phố hoặc số nhà.
        /// </param>
        /// <param name="locality">
        /// Thành phố hoặc đơn vị địa lý tương đương.
        /// </param>
        /// <param name="addressLine2">
        /// Thông tin địa chỉ bổ sung, nếu có.
        /// </param>
        /// <param name="dependentLocality">
        /// Phường, xã, quận, huyện hoặc khu vực tương đương.
        /// </param>
        /// <param name="administrativeArea">
        /// Tỉnh hoặc thành phố trực thuộc trung ương, nếu có.
        /// </param>
        /// <param name="postalCode">
        /// Mã bưu chính, nếu có.
        /// </param>
        /// <returns>Một Address Value Object hợp lệ tại Việt Nam.</returns>
        public static Address CreateVietnameseAddress(
            string addressLine1,
            string locality,
            string? addressLine2 = null,
            string? dependentLocality = null,
            string? administrativeArea = null,
            string? postalCode = null)
        {
            return Create(
                addressLine1: addressLine1,
                locality: locality,
                countryCode: "VN",
                addressLine2: addressLine2,
                dependentLocality: dependentLocality,
                administrativeArea: administrativeArea,
                postalCode: postalCode);
        }

        /// <summary>
        /// Tạo một bản sao của Address với địa chỉ chi tiết mới.
        /// 
        /// Đối tượng hiện tại không bị thay đổi.
        /// </summary>
        /// <param name="addressLine1">
        /// Địa chỉ chi tiết mới.
        /// </param>
        /// <returns>Một Address Value Object mới.</returns>
        public Address WithAddressLine1(string addressLine1)
        {
            return Create(
                addressLine1,
                Locality,
                CountryCode,
                AddressLine2,
                DependentLocality,
                AdministrativeArea,
                PostalCode);
        }

        /// <summary>
        /// Tạo một bản sao của Address với thành phố mới.
        /// 
        /// Đối tượng hiện tại không bị thay đổi.
        /// </summary>
        /// <param name="locality">
        /// Thành phố hoặc đơn vị địa lý mới.
        /// </param>
        /// <returns>Một Address Value Object mới.</returns>
        public Address WithLocality(string locality)
        {
            return Create(
                AddressLine1,
                locality,
                CountryCode,
                AddressLine2,
                DependentLocality,
                AdministrativeArea,
                PostalCode);
        }

        /// <summary>
        /// Tạo một bản sao của Address với mã quốc gia mới.
        /// 
        /// Đối tượng hiện tại không bị thay đổi.
        /// </summary>
        /// <param name="countryCode">
        /// Mã quốc gia mới theo ISO 3166-1 alpha-2.
        /// </param>
        /// <returns>Một Address Value Object mới.</returns>
        public Address WithCountryCode(string countryCode)
        {
            return Create(
                AddressLine1,
                Locality,
                countryCode,
                AddressLine2,
                DependentLocality,
                AdministrativeArea,
                PostalCode);
        }

        /// <summary>
        /// Kiểm tra một trường bắt buộc có hợp lệ hay không.
        /// </summary>
        /// <param name="value">Giá trị cần kiểm tra.</param>
        /// <param name="parameterName">Tên tham số.</param>
        /// <param name="maxLength">Độ dài tối đa cho phép.</param>
        /// <exception cref="ArgumentException">
        /// Ném ra khi giá trị để trống hoặc vượt quá độ dài cho phép.
        /// </exception>
        private static void ValidateRequiredField(
            string value,
            string parameterName,
            int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(
                    "Thành phần địa chỉ bắt buộc không được để trống.",
                    parameterName);
            }

            if (value.Trim().Length > maxLength)
            {
                throw new ArgumentException(
                    $"Thành phần địa chỉ không được vượt quá {maxLength} ký tự.",
                    parameterName);
            }
        }

        /// <summary>
        /// Kiểm tra một trường tùy chọn có hợp lệ hay không.
        /// </summary>
        /// <param name="value">Giá trị cần kiểm tra.</param>
        /// <param name="parameterName">Tên tham số.</param>
        /// <param name="maxLength">Độ dài tối đa cho phép.</param>
        /// <exception cref="ArgumentException">
        /// Ném ra khi giá trị vượt quá độ dài cho phép.
        /// </exception>
        private static void ValidateOptionalField(
            string? value,
            string parameterName,
            int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            if (value.Trim().Length > maxLength)
            {
                throw new ArgumentException(
                    $"Thành phần địa chỉ không được vượt quá {maxLength} ký tự.",
                    parameterName);
            }
        }

        /// <summary>
        /// Kiểm tra mã quốc gia có đúng định dạng ISO 3166-1 alpha-2
        /// về mặt cú pháp hay không.
        /// 
        /// Phương thức này kiểm tra định dạng hai chữ cái,
        /// không xác minh mã có tồn tại trong danh sách quốc gia
        /// chính thức hay không.
        /// </summary>
        /// <param name="countryCode">Mã quốc gia cần kiểm tra.</param>
        /// <exception cref="ArgumentException">
        /// Ném ra khi mã quốc gia không đúng định dạng.
        /// </exception>
        private static void ValidateCountryCode(string countryCode)
        {
            if (string.IsNullOrWhiteSpace(countryCode))
            {
                throw new ArgumentException(
                    "Mã quốc gia không được để trống.",
                    nameof(countryCode));
            }

            countryCode = countryCode.Trim().ToUpperInvariant();

            if (countryCode.Length != MaxCountryCodeLength ||
                !CountryCodeRegex.IsMatch(countryCode))
            {
                throw new ArgumentException(
                    "Mã quốc gia phải có đúng 2 chữ cái theo ISO 3166-1 alpha-2.",
                    nameof(countryCode));
            }
        }

        /// <summary>
        /// Chuẩn hóa một giá trị chuỗi bắt buộc.
        /// 
        /// Loại bỏ khoảng trắng ở hai đầu chuỗi.
        /// </summary>
        /// <param name="value">Giá trị cần chuẩn hóa.</param>
        /// <returns>Giá trị đã được chuẩn hóa.</returns>
        private static string NormalizeRequiredValue(string value)
        {
            return value.Trim();
        }

        /// <summary>
        /// Chuẩn hóa một giá trị chuỗi tùy chọn.
        /// Chuỗi null, rỗng hoặc chỉ chứa khoảng trắng
        /// sẽ được chuyển thành null.
        /// Các giá trị hợp lệ sẽ được loại bỏ khoảng trắng ở hai đầu.
        /// </summary>
        /// <param name="value">Giá trị cần chuẩn hóa.</param>
        /// <returns>Giá trị đã chuẩn hóa hoặc null.</returns>
        private static string? NormalizeOptionalValue(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}