using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InternshipManagement.Migrations
{
    /// <inheritdoc />
    public partial class SyncModel_20250922 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "DeTai",
                columns: new[] { "madt", "hocky", "kinhphi", "magv", "namhoc", "noithucTap", "soluongtoida", "tendt" },
                values: new object[,]
                {
                    { "DT461", (byte)1, 22, 1, "2019-2020", "FPT Software", 3, "Nền tảng quản lý khóa học microservices (.NET + React)" },
                    { "DT462", (byte)1, 24, 1, "2019-2020", "VNG Cloud", 4, "Trợ lý học tập dùng LLM (RAG + Azure OpenAI)" },
                    { "DT463", (byte)1, 18, 1, "2019-2020", "NashTech VN", 3, "Hệ thống chấm bài lập trình tự động (Online Judge)" },
                    { "DT464", (byte)1, 17, 1, "2019-2020", "Viettel Solutions", 3, "Dashboard IoT giám sát phòng lab (MQTT + Timeseries DB)" },
                    { "DT465", (byte)1, 20, 1, "2019-2020", "VNPT Data", 4, "Phân tích dữ liệu sinh viên & dự báo rủi ro học tập (BI/ML)" },
                    { "DT466", (byte)1, 19, 1, "2019-2020", "Axon Active", 3, "Cổng tuyển sinh số đa kênh (Next.js + Keycloak SSO)" },
                    { "DT467", (byte)2, 21, 1, "2019-2020", "Zalo AI", 4, "Chatbot hỗ trợ sinh viên (RAG + Vector DB + LangChain)" },
                    { "DT468", (byte)2, 18, 1, "2019-2020", "TopCV", 3, "Kết nối thực tập & việc làm (Matching + Recommender)" },
                    { "DT469", (byte)2, 23, 1, "2019-2020", "CMC Global", 4, "Đăng ký học phần chịu tải cao (CQRS + Event Sourcing)" },
                    { "DT470", (byte)2, 16, 1, "2019-2020", "VinAI", 3, "Điểm danh nhận diện khuôn mặt (Edge AI + ONNX)" },
                    { "DT471", (byte)2, 15, 1, "2019-2020", "VNU-HCM Alumni", 3, "Cổng thông tin cựu sinh viên & mentor" },
                    { "DT472", (byte)2, 17, 1, "2019-2020", "HCMUT", 3, "Chấm thi trắc nghiệm bảo mật (Scan + Anti-cheat)" },
                    { "DT473", (byte)2, 19, 1, "2019-2020", "VNU-HCM", 3, "Tối ưu xếp thời khóa biểu (ILP/Heuristic)" },
                    { "DT474", (byte)3, 20, 1, "2019-2020", "VietAI", 4, "E-Library tìm kiếm ngữ nghĩa (Elastic + BERT)" },
                    { "DT475", (byte)3, 18, 1, "2019-2020", "Be Group", 3, "Hệ thống quản lý thực tập (Portal + Mobile App)" },
                    { "DT476", (byte)3, 22, 1, "2019-2020", "VNPT Data", 4, "ETL hồ sơ học tập & kho dữ liệu (DWH)" },
                    { "DT477", (byte)3, 17, 1, "2019-2020", "HCMUS", 3, "Chấm điểm đồ án bằng rubric (Workflow + Review)" },
                    { "DT478", (byte)3, 21, 1, "2019-2020", "VNG Cloud", 4, "Giám sát hạ tầng học tập (K8s + Prometheus + Grafana)" },
                    { "DT479", (byte)3, 16, 1, "2019-2020", "VNExpress Data Lab", 3, "Hệ thống phản hồi chất lượng dạy học (Text Mining)" },
                    { "DT480", (byte)3, 23, 1, "2019-2020", "FPT Software", 4, "Proctoring thi trực tuyến (FaceID + Liveness)" },
                    { "DT481", (byte)3, 19, 1, "2019-2020", "HCMUE", 3, "Cố vấn học tập thông minh (Rule-based + ML)" },
                    { "DT482", (byte)1, 18, 1, "2021-2022", "TopDev", 3, "Portal tuyển dụng thực tập sinh (ATS + Scoring)" },
                    { "DT483", (byte)1, 17, 1, "2021-2022", "VCCorp", 3, "Chấm báo cáo trình bày tự động (NLP + Layout)" },
                    { "DT484", (byte)1, 20, 1, "2021-2022", "Vietcombank Digital", 4, "Quản trị học bổng & xếp hạng (BI Dashboard)" },
                    { "DT485", (byte)1, 16, 1, "2021-2022", "HUST", 3, "Tối ưu phòng thi & coi thi (ILP)" },
                    { "DT486", (byte)1, 21, 1, "2021-2022", "VNU-HCM", 4, "Kho học liệu số & chống đạo văn (NLP)" },
                    { "DT487", (byte)2, 19, 1, "2021-2022", "FPT Software", 3, "Đăng ký tín chỉ tối ưu (Heuristic + Constraint)" },
                    { "DT488", (byte)2, 18, 1, "2021-2022", "Axon Active", 3, "Chợ đề tài đồ án & matching giảng viên–sinh viên" },
                    { "DT489", (byte)2, 17, 1, "2021-2022", "VNU-HCM", 3, "Hồ sơ năng lực sinh viên (e-Portfolio)" },
                    { "DT490", (byte)2, 22, 1, "2021-2022", "CMC Global", 4, "SSO toàn hệ thống (Keycloak + OIDC) & phân quyền RBAC" },
                    { "DT491", (byte)2, 20, 1, "2021-2022", "Viettel Solutions", 4, "Giám sát lớp học thông minh (IoT + CV)" },
                    { "DT492", (byte)2, 18, 1, "2021-2022", "Udemy VN", 3, "Tư vấn lộ trình học & chứng chỉ (Recommender)" },
                    { "DT493", (byte)3, 16, 1, "2021-2022", "HCMUT", 3, "Hệ thống đánh giá giảng dạy (Survey + NLP Insight)" },
                    { "DT494", (byte)3, 21, 1, "2021-2022", "VNPT Data", 4, "Data Lake học tập & chuẩn hóa pipeline (Airflow)" },
                    { "DT495", (byte)3, 23, 1, "2021-2022", "VNG Cloud", 4, "Giảng đường ảo 3D (WebGL + XR)" },
                    { "DT496", (byte)3, 17, 1, "2021-2022", "NashTech VN", 3, "Chống gian lận thi code (AST + Similarity)" },
                    { "DT497", (byte)3, 18, 1, "2021-2022", "HCMUTE", 3, "Xếp lịch bảo trì phòng máy & thiết bị (Rule Engine)" },
                    { "DT498", (byte)3, 20, 1, "2021-2022", "VinBigData", 4, "Cảnh báo sớm rủi ro học vụ (Early Warning)" },
                    { "DT499", (byte)3, 19, 1, "2021-2022", "MoMo", 3, "Cổng minh bạch học phí & hóa đơn (FinTech)" },
                    { "DT500", (byte)1, 20, 1, "2022-2023", "OpenEdu", 4, "Hồ sơ học tập suốt đời (LRS + xAPI)" },
                    { "DT501", (byte)1, 18, 1, "2022-2023", "VNU-HCM", 3, "Quản trị đề cương & chuẩn đầu ra (OBE)" },
                    { "DT502", (byte)1, 19, 1, "2022-2023", "Zalo AI", 3, "Chấm bài luận bằng rubric + gợi ý phản hồi (NLP)" },
                    { "DT503", (byte)1, 21, 1, "2022-2023", "VNG Cloud", 4, "Giám sát SLA dịch vụ đào tạo (SRE + Grafana)" },
                    { "DT504", (byte)1, 17, 1, "2022-2023", "HCMUT", 3, "Phân bổ phòng học tối ưu (ILP + Constraint)" },
                    { "DT505", (byte)1, 18, 1, "2022-2023", "TopCV", 3, "Cổng kiến tập doanh nghiệp (Matching theo kỹ năng)" },
                    { "DT506", (byte)2, 22, 1, "2022-2023", "FPT Software", 4, "LMS tích hợp proctoring & plagiarism (SDK)" },
                    { "DT507", (byte)2, 18, 1, "2022-2023", "VNExpress Data Lab", 3, "Kho học liệu số hóa (OCR + Search)" },
                    { "DT508", (byte)2, 24, 1, "2022-2023", "Viettel Solutions", 4, "Nền tảng MOOC nội bộ (Streaming + CDN)" },
                    { "DT509", (byte)2, 16, 1, "2022-2023", "HCMUS", 3, "Tối ưu trực nhật & mượn phòng lab (Workflow)" },
                    { "DT510", (byte)2, 19, 1, "2022-2023", "VCCorp", 3, "Chấm điểm bài thuyết trình (Audio+NLP)" },
                    { "DT511", (byte)2, 17, 1, "2022-2023", "Be Group", 3, "Hệ thống lịch thực tập & nhắc việc (Mobile)" },
                    { "DT512", (byte)2, 20, 1, "2022-2023", "HUST", 4, "Xếp lịch thi tập trung (Constraint + ILP)" },
                    { "DT513", (byte)2, 18, 1, "2022-2023", "VNU-HCM", 3, "Phân tích năng lực giảng viên (BI + KPI)" },
                    { "DT514", (byte)3, 21, 1, "2022-2023", "VNPT Data", 4, "Hồ sơ năng lực số (Verifiable Credential)" },
                    { "DT515", (byte)3, 18, 1, "2022-2023", "MoMo", 3, "Cố vấn chọn chuyên ngành (Recommender)" },
                    { "DT516", (byte)3, 20, 1, "2022-2023", "VNG Cloud", 4, "Giám sát lớp học thông minh (Camera + Privacy)" },
                    { "DT517", (byte)3, 17, 1, "2022-2023", "HCMUT", 3, "Quản trị đề tài tốt nghiệp (Workflow + Rubric)" },
                    { "DT518", (byte)3, 16, 1, "2022-2023", "VCCorp", 3, "Phân tích phản hồi môn học (Sentiment Mining)" },
                    { "DT519", (byte)3, 19, 1, "2022-2023", "TopCV", 3, "Hệ thống KPI thực tập & mentor (BI)" },
                    { "DT520", (byte)3, 22, 1, "2022-2023", "FPT Software", 4, "Data Catalog cho học liệu (OpenMetadata)" },
                    { "DT521", (byte)1, 21, 1, "2023-2024", "VNG Cloud", 4, "Quản trị máy chủ dạy–học (K8s + GitOps)" },
                    { "DT522", (byte)1, 18, 1, "2023-2024", "CMC Global", 3, "Cổng dịch vụ sinh viên một cửa (Portal + Queue)" },
                    { "DT523", (byte)1, 20, 1, "2023-2024", "NashTech VN", 4, "Chấm bài lập trình containerized (Sandbox + Judge)" },
                    { "DT524", (byte)1, 17, 1, "2023-2024", "Viettel Solutions", 3, "Giám sát thiết bị phòng lab (IoT + Grafana)" },
                    { "DT525", (byte)1, 19, 1, "2023-2024", "VNU-HCM", 3, "Cổng đăng ký xét tốt nghiệp (Workflow + E-Sign)" },
                    { "DT526", (byte)1, 16, 1, "2023-2024", "HCMUT", 3, "Bản đồ số cơ sở vật chất (GIS + Campus Map)" },
                    { "DT527", (byte)2, 18, 1, "2023-2024", "HUST", 3, "Tối ưu lịch thi vấn đáp (ILP + Timeslot)" },
                    { "DT528", (byte)2, 19, 1, "2023-2024", "VNPT Data", 3, "Phân tích hiệu quả môn học (Cohort Analysis)" },
                    { "DT529", (byte)2, 23, 1, "2023-2024", "Zalo AI", 4, "Trợ giảng ảo cho lớp lập trình (LLM + Code Review)" },
                    { "DT530", (byte)2, 17, 1, "2023-2024", "TopCV", 3, "Quản trị thực tập theo năng lực (Skill Matrix)" },
                    { "DT531", (byte)2, 22, 1, "2023-2024", "FPT Software", 4, "Đồng bộ dữ liệu đa hệ (ESB + CDC)" },
                    { "DT532", (byte)2, 16, 1, "2023-2024", "Axon Active", 3, "Theo dõi tiến độ đồ án (Burndown + Kanban)" },
                    { "DT533", (byte)2, 20, 1, "2023-2024", "VNG Cloud", 4, "Trung tâm dịch vụ học thuật (ITSM for EDU)" },
                    { "DT534", (byte)3, 19, 1, "2023-2024", "VNU-HCM", 3, "Đánh giá chuẩn đầu ra tự động (NLP + Rubric)" },
                    { "DT535", (byte)3, 21, 1, "2023-2024", "VNPT Data", 4, "Kho dữ liệu minh chứng kiểm định (DWH + Lineage)" },
                    { "DT536", (byte)3, 18, 1, "2023-2024", "Viettel Solutions", 3, "Hệ thống lịch phòng thông minh (IoT + Booking)" },
                    { "DT537", (byte)3, 17, 1, "2023-2024", "HCMUE", 3, "Tính điểm rèn luyện bán tự động (Rule + Evidence)" },
                    { "DT538", (byte)3, 22, 1, "2023-2024", "VNG Cloud", 4, "Phân tích truy cập bất thường (SIEM + ML)" },
                    { "DT539", (byte)3, 16, 1, "2023-2024", "VCCorp", 3, "Tra cứu văn bản – công văn (OCR + Search)" },
                    { "DT540", (byte)3, 18, 1, "2023-2024", "Be Group", 3, "Cổng workshop & seminar (Registration + Ticket)" },
                    { "DT541", (byte)3, 23, 1, "2023-2024", "FPT Software", 4, "CI/CD cho hạ tầng dạy–học (ArgoCD + Helm)" },
                    { "DT542", (byte)1, 22, 1, "2024-2025", "Zalo AI", 4, "Hệ thống hỏi đáp học thuật (RAG + Retrieval Filter)" },
                    { "DT543", (byte)1, 17, 1, "2024-2025", "VNU-HCM", 3, "Quản trị lịch cố vấn học tập (Advisor Portal)" },
                    { "DT544", (byte)1, 18, 1, "2024-2025", "Vietcombank Digital", 3, "Cảnh báo nợ học phí & nhắc hạn (FinOps)" },
                    { "DT545", (byte)1, 19, 1, "2024-2025", "TopCV", 3, "Đề xuất lộ trình chứng chỉ CNTT (Career Path)" },
                    { "DT546", (byte)1, 20, 1, "2024-2025", "Viettel Solutions", 4, "Theo dõi chất lượng mạng lớp học (AP + NQI)" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT461");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT462");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT463");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT464");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT465");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT466");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT467");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT468");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT469");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT470");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT471");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT472");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT473");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT474");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT475");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT476");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT477");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT478");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT479");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT480");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT481");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT482");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT483");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT484");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT485");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT486");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT487");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT488");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT489");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT490");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT491");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT492");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT493");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT494");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT495");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT496");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT497");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT498");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT499");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT500");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT501");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT502");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT503");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT504");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT505");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT506");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT507");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT508");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT509");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT510");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT511");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT512");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT513");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT514");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT515");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT516");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT517");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT518");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT519");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT520");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT521");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT522");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT523");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT524");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT525");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT526");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT527");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT528");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT529");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT530");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT531");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT532");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT533");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT534");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT535");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT536");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT537");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT538");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT539");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT540");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT541");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT542");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT543");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT544");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT545");

            migrationBuilder.DeleteData(
                table: "DeTai",
                keyColumn: "madt",
                keyValue: "DT546");
        }
    }
}
