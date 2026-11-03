using System.Text.RegularExpressions;

namespace MKS.Shared.Domain.Shared.Common.ValueObjects
{
    /// <summary>
    /// Đại diện cho một hình ảnh trong hệ thống.
    /// 
    /// Đây là Value Object bất biến, được sử dụng để lưu trữ thông tin tham chiếu
    /// và metadata của hình ảnh, không lưu trữ nội dung binary trực tiếp.
    /// 
    /// Hình ảnh có thể được lưu trữ trên Cloud Storage, CDN hoặc hệ thống file.
    /// 
    /// Hai đối tượng Image được xem là bằng nhau khi tất cả thuộc tính của chúng
    /// có giá trị bằng nhau.
    /// </summary>
    public sealed record Image
    {
        private const int MaxUrlLength = 2048;
        private const int MaxFileNameLength = 255;
        private const int MaxAltLength = 500;
        private const int MaxContentTypeLength = 100;
        private const int MaxStorageKeyLength = 1024;

        private static readonly Regex FileNameRegex = new(
            @"^[^<>:""/\\|?*\x00-\x1F]+$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>
        /// URL hoặc đường dẫn truy cập hình ảnh.
        /// 
        /// Có thể là URL tuyệt đối hoặc đường dẫn tương đối tùy theo
        /// chiến lược lưu trữ và phân phối hình ảnh của hệ thống.
        /// </summary>
        public string Url { get; private init; }

        /// <summary>
        /// Tên file gốc của hình ảnh.
        /// 
        /// Ví dụ: product-image-001.webp.
        /// Thuộc tính này có thể không có giá trị nếu hệ thống không lưu tên file gốc.
        /// </summary>
        public string? FileName { get; private init; }

        /// <summary>
        /// Văn bản mô tả hình ảnh dành cho accessibility và SEO.
        /// 
        /// Ví dụ: "Áo sơ mi nam màu trắng".
        /// Có thể null nếu chưa có mô tả.
        /// </summary>
        public string? Alt { get; private init; }

        /// <summary>
        /// MIME type của hình ảnh.
        /// 
        /// Ví dụ: image/jpeg, image/png, image/webp hoặc image/avif.
        /// Giá trị này nên được xác minh tại Application hoặc Infrastructure
        /// khi tiếp nhận file upload.
        /// </summary>
        public string? ContentType { get; private init; }

        /// <summary>
        /// Dung lượng hình ảnh tính bằng bytes.
        /// 
        /// Giá trị phải lớn hơn 0 nếu được cung cấp.
        /// </summary>
        public long? FileSize { get; private init; }

        /// <summary>
        /// Chiều rộng hình ảnh tính bằng pixel.
        /// 
        /// Giá trị phải lớn hơn 0 nếu được cung cấp.
        /// </summary>
        public int? Width { get; private init; }

        /// <summary>
        /// Chiều cao hình ảnh tính bằng pixel.
        /// 
        /// Giá trị phải lớn hơn 0 nếu được cung cấp.
        /// </summary>
        public int? Height { get; private init; }

        /// <summary>
        /// Khóa định danh hình ảnh trong hệ thống lưu trữ.
        /// 
        /// Ví dụ: products/2026/11/product-001.webp.
        /// 
        /// Không nên lưu thông tin xác thực hoặc access token trong thuộc tính này.
        /// </summary>
        public string? StorageKey { get; private init; }

        /// <summary>
        /// Khởi tạo một Image Value Object.
        /// 
        /// Constructor private nhằm đảm bảo mọi đối tượng Image
        /// đều được tạo thông qua các factory method có validation.
        /// </summary>
        private Image(
            string url,
            string? fileName,
            string? alt,
            string? contentType,
            long? fileSize,
            int? width,
            int? height,
            string? storageKey)
        {
            Url = url;
            FileName = fileName;
            Alt = alt;
            ContentType = contentType;
            FileSize = fileSize;
            Width = width;
            Height = height;
            StorageKey = storageKey;
        }

        /// <summary>
        /// Tạo một Image Value Object từ URL và metadata.
        /// 
        /// Đây là factory method chính được sử dụng khi tạo hình ảnh
        /// từ dữ liệu đã được xác định trong Domain.
        /// </summary>
        /// <param name="url">URL hoặc đường dẫn truy cập hình ảnh.</param>
        /// <param name="fileName">Tên file gốc, nếu có.</param>
        /// <param name="alt">Văn bản mô tả hình ảnh, nếu có.</param>
        /// <param name="contentType">MIME type của hình ảnh, nếu có.</param>
        /// <param name="fileSize">Dung lượng hình ảnh tính bằng bytes, nếu có.</param>
        /// <param name="width">Chiều rộng hình ảnh tính bằng pixel, nếu có.</param>
        /// <param name="height">Chiều cao hình ảnh tính bằng pixel, nếu có.</param>
        /// <param name="storageKey">Khóa định danh trong hệ thống lưu trữ, nếu có.</param>
        /// <returns>Một Image Value Object hợp lệ.</returns>
        /// <exception cref="ArgumentException">
        /// Ném ra khi URL hoặc metadata không hợp lệ.
        /// </exception>
        public static Image Create(
            string url,
            string? fileName = null,
            string? alt = null,
            string? contentType = null,
            long? fileSize = null,
            int? width = null,
            int? height = null,
            string? storageKey = null)
        {
            ValidateUrl(url);
            ValidateFileName(fileName);
            ValidateAlt(alt);
            ValidateContentType(contentType);
            ValidateFileSize(fileSize);
            ValidateDimensions(width, height);
            ValidateStorageKey(storageKey);

            return new Image(
                url.Trim(),
                NormalizeOptionalValue(fileName),
                NormalizeOptionalValue(alt),
                NormalizeOptionalValue(contentType)?.ToLowerInvariant(),
                fileSize,
                width,
                height,
                NormalizeOptionalValue(storageKey));
        }

        /// <summary>
        /// Tạo Image Value Object chỉ từ URL.
        /// 
        /// Sử dụng khi hệ thống chỉ cần tham chiếu đến hình ảnh
        /// mà không cần lưu metadata bổ sung.
        /// </summary>
        /// <param name="url">URL hoặc đường dẫn truy cập hình ảnh.</param>
        /// <returns>Một Image Value Object hợp lệ.</returns>
        /// <exception cref="ArgumentException">
        /// Ném ra khi URL không hợp lệ.
        /// </exception>
        public static Image FromUrl(string url)
        {
            return Create(url);
        }

        /// <summary>
        /// Tạo một bản sao của Image với văn bản mô tả mới.
        /// 
        /// Đối tượng hiện tại không bị thay đổi.
        /// </summary>
        /// <param name="alt">Văn bản mô tả mới.</param>
        /// <returns>Một Image Value Object mới với mô tả được cập nhật.</returns>
        /// <exception cref="ArgumentException">
        /// Ném ra khi văn bản mô tả không hợp lệ.
        /// </exception>
        public Image WithAlt(string? alt)
        {
            return Create(
                Url,
                FileName,
                alt,
                ContentType,
                FileSize,
                Width,
                Height,
                StorageKey);
        }

        /// <summary>
        /// Tạo một bản sao của Image với URL mới.
        /// 
        /// Đối tượng hiện tại không bị thay đổi.
        /// </summary>
        /// <param name="url">URL hoặc đường dẫn mới.</param>
        /// <returns>Một Image Value Object mới.</returns>
        /// <exception cref="ArgumentException">
        /// Ném ra khi URL mới không hợp lệ.
        /// </exception>
        public Image WithUrl(string url)
        {
            return Create(
                url,
                FileName,
                Alt,
                ContentType,
                FileSize,
                Width,
                Height,
                StorageKey);
        }

        /// <summary>
        /// Kiểm tra URL có hợp lệ hay không.
        /// 
        /// URL tuyệt đối phải sử dụng HTTP hoặc HTTPS.
        /// Đường dẫn tương đối được chấp nhận để hỗ trợ Storage Key
        /// hoặc đường dẫn nội bộ của hệ thống.
        /// </summary>
        /// <param name="url">URL cần kiểm tra.</param>
        /// <exception cref="ArgumentException">
        /// Ném ra khi URL không hợp lệ.
        /// </exception>
        private static void ValidateUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException(
                    "URL hình ảnh không được để trống.",
                    nameof(url));
            }

            url = url.Trim();

            if (url.Length > MaxUrlLength)
            {
                throw new ArgumentException(
                    $"URL hình ảnh không được vượt quá {MaxUrlLength} ký tự.",
                    nameof(url));
            }

            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                if (uri.Scheme != Uri.UriSchemeHttp &&
                    uri.Scheme != Uri.UriSchemeHttps)
                {
                    throw new ArgumentException(
                        "URL hình ảnh chỉ hỗ trợ giao thức HTTP hoặc HTTPS.",
                        nameof(url));
                }

                if (string.IsNullOrWhiteSpace(uri.Host))
                {
                    throw new ArgumentException(
                        "URL hình ảnh phải có hostname hợp lệ.",
                        nameof(url));
                }

                return;
            }

            if (url.StartsWith("//", StringComparison.Ordinal) ||
                url.StartsWith("\\\\", StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "URL hình ảnh không được sử dụng network-path reference.",
                    nameof(url));
            }

            if (url.Contains('\\'))
            {
                throw new ArgumentException(
                    "Đường dẫn hình ảnh không được chứa ký tự backslash.",
                    nameof(url));
            }
        }

        /// <summary>
        /// Kiểm tra tên file có hợp lệ hay không.
        /// </summary>
        /// <param name="fileName">Tên file cần kiểm tra.</param>
        /// <exception cref="ArgumentException">
        /// Ném ra khi tên file không hợp lệ.
        /// </exception>
        private static void ValidateFileName(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return;
            }

            fileName = fileName.Trim();

            if (fileName.Length > MaxFileNameLength)
            {
                throw new ArgumentException(
                    $"Tên file không được vượt quá {MaxFileNameLength} ký tự.",
                    nameof(fileName));
            }

            if (!FileNameRegex.IsMatch(fileName))
            {
                throw new ArgumentException(
                    "Tên file chứa ký tự không hợp lệ.",
                    nameof(fileName));
            }
        }

        /// <summary>
        /// Kiểm tra văn bản mô tả hình ảnh có hợp lệ hay không.
        /// </summary>
        /// <param name="alt">Văn bản mô tả cần kiểm tra.</param>
        /// <exception cref="ArgumentException">
        /// Ném ra khi văn bản mô tả vượt quá giới hạn cho phép.
        /// </exception>
        private static void ValidateAlt(string? alt)
        {
            if (alt is null)
            {
                return;
            }

            if (alt.Length > MaxAltLength)
            {
                throw new ArgumentException(
                    $"Văn bản mô tả hình ảnh không được vượt quá {MaxAltLength} ký tự.",
                    nameof(alt));
            }
        }

        /// <summary>
        /// Kiểm tra MIME type có hợp lệ hay không.
        /// </summary>
        /// <param name="contentType">MIME type cần kiểm tra.</param>
        /// <exception cref="ArgumentException">
        /// Ném ra khi MIME type không hợp lệ.
        /// </exception>
        private static void ValidateContentType(string? contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType))
            {
                return;
            }

            contentType = contentType.Trim();

            if (contentType.Length > MaxContentTypeLength)
            {
                throw new ArgumentException(
                    $"MIME type không được vượt quá {MaxContentTypeLength} ký tự.",
                    nameof(contentType));
            }

            if (!contentType.StartsWith(
                    "image/",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    "ContentType phải là MIME type của hình ảnh.",
                    nameof(contentType));
            }
        }

        /// <summary>
        /// Kiểm tra dung lượng hình ảnh có hợp lệ hay không.
        /// </summary>
        /// <param name="fileSize">Dung lượng hình ảnh tính bằng bytes.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Ném ra khi dung lượng nhỏ hơn hoặc bằng 0.
        /// </exception>
        private static void ValidateFileSize(long? fileSize)
        {
            if (fileSize.HasValue && fileSize.Value <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(fileSize),
                    "Dung lượng hình ảnh phải lớn hơn 0 bytes.");
            }
        }

        /// <summary>
        /// Kiểm tra chiều rộng và chiều cao hình ảnh có hợp lệ hay không.
        /// </summary>
        /// <param name="width">Chiều rộng hình ảnh tính bằng pixel.</param>
        /// <param name="height">Chiều cao hình ảnh tính bằng pixel.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Ném ra khi chiều rộng hoặc chiều cao nhỏ hơn hoặc bằng 0.
        /// </exception>
        private static void ValidateDimensions(
            int? width,
            int? height)
        {
            if (width.HasValue && width.Value <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(width),
                    "Chiều rộng hình ảnh phải lớn hơn 0 pixel.");
            }

            if (height.HasValue && height.Value <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(height),
                    "Chiều cao hình ảnh phải lớn hơn 0 pixel.");
            }
        }

        /// <summary>
        /// Kiểm tra Storage Key có hợp lệ hay không.
        /// </summary>
        /// <param name="storageKey">Storage Key cần kiểm tra.</param>
        /// <exception cref="ArgumentException">
        /// Ném ra khi Storage Key không hợp lệ.
        /// </exception>
        private static void ValidateStorageKey(string? storageKey)
        {
            if (string.IsNullOrWhiteSpace(storageKey))
            {
                return;
            }

            storageKey = storageKey.Trim();

            if (storageKey.Length > MaxStorageKeyLength)
            {
                throw new ArgumentException(
                    $"Storage Key không được vượt quá {MaxStorageKeyLength} ký tự.",
                    nameof(storageKey));
            }

            if (storageKey.StartsWith("/", StringComparison.Ordinal) ||
                storageKey.StartsWith("\\", StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "Storage Key không được là đường dẫn tuyệt đối.",
                    nameof(storageKey));
            }

            if (storageKey.Contains('\\'))
            {
                throw new ArgumentException(
                    "Storage Key không được chứa ký tự backslash.",
                    nameof(storageKey));
            }

            if (Uri.TryCreate(storageKey, UriKind.Absolute, out _))
            {
                throw new ArgumentException(
                    "Storage Key chỉ được chứa khóa lưu trữ, không phải URL.",
                    nameof(storageKey));
            }
        }

        /// <summary>
        /// Chuẩn hóa giá trị chuỗi tùy chọn.
        /// 
        /// Chuỗi null, rỗng hoặc chỉ chứa khoảng trắng sẽ được chuyển thành null.
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