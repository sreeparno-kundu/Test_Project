using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Newtonsoft.Json;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Test_Project.Models;


public class HomeController : Controller
{
    string connectionString = "Data Source=SREEPARNO;Initial Catalog=sreeparno_db;User ID=sa;Password=mcc#1234;";


    public ActionResult Index()
    {
        if (Session["UserId"] == null)
        {
            return RedirectToAction("About");
        }

        ViewBag.User = Session["UserId"];
        return View();
    }

    [AllowAnonymous]
    public ActionResult About()
    {

        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    public ActionResult Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_UserLogin", con);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserId", model.UserId);
                cmd.Parameters.AddWithValue("@Password", model.Password);

                con.Open();
                int count = (int)cmd.ExecuteScalar();
                con.Close();

                if (count > 0)
                {
                    Session["UserId"] = model.UserId;
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.Message = "Invalid credentials!";
                }
            }
        }
        return View("About", model);
    }

    [HttpPost]
    [AllowAnonymous]
    public JsonResult ApiLogin(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_UserLogin", con);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserId", model.UserId);
                cmd.Parameters.AddWithValue("@Password", model.Password);

                con.Open();
                int count = (int)cmd.ExecuteScalar();
                con.Close();

                if (count > 0)
                {
                    Session["UserId"] = model.UserId;
                    return Json(new { success = true, message = "Login successful!" });
                }
                else
                {
                    return Json(new { success = false, message = "Invalid credentials!" });
                }
            }
        }
        return Json(new { success = false, message = "Validation failed!" });
    }

    // New AJAX-based endpoint for StudentList
    public ActionResult StudentList()
    {
        if (Session["UserId"] == null)
        {
            return RedirectToAction("About");
        }

        ViewBag.User = Session["UserId"];
        // The view will be populated via an AJAX call from JavaScript
        return View();
    }

    [HttpGet]
    public JsonResult GetStudents(string keyword = "", int pageNumber = 1, int pageSize = 10)
    {
        List<Student> students = new List<Student>();
        int totalCount = 0;

        using (SqlConnection con = new SqlConnection(connectionString))
        {
            SqlCommand cmd = new SqlCommand("sp_SearchStudents", con);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Keyword", (object)keyword ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PageNumber", pageNumber);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);

            con.Open();
            using (SqlDataReader rdr = cmd.ExecuteReader())
            {
                // First result set: paged data
                while (rdr.Read())
                {
                    students.Add(new Student
                    {
                        Id = Convert.ToInt32(rdr["Id"]),
                        Name = rdr["Name"].ToString(),
                        Age = Convert.ToInt32(rdr["Age"]),
                        Email = rdr["Email"].ToString()
                    });
                }

                // Move to second result set: total count
                if (rdr.NextResult() && rdr.Read())
                {
                    totalCount = Convert.ToInt32(rdr["TotalCount"]);
                }
            }
        }

        return Json(new { success = true, data = students, totalCount = totalCount }, JsonRequestBehavior.AllowGet);
    }


    [HttpPost]
    public JsonResult InsertStudent(Student student)
    {
        try
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_InsertStudent", con);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Name", student.Name);
                cmd.Parameters.AddWithValue("@Age", student.Age);
                cmd.Parameters.AddWithValue("@Email", student.Email);
                con.Open();
                cmd.ExecuteNonQuery();
            }
            return Json(new { success = true, message = "Student inserted successfully." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Error: " + ex.Message });
        }
    }

    [HttpPost]
    public JsonResult UpdateStudent(Student student)
    {
        try
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_UpdateStudent", con);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id", student.Id);
                cmd.Parameters.AddWithValue("@Name", student.Name);
                cmd.Parameters.AddWithValue("@Age", student.Age);
                cmd.Parameters.AddWithValue("@Email", student.Email);
                con.Open();
                cmd.ExecuteNonQuery();
            }
            return Json(new { success = true, message = "Student updated successfully." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Error: " + ex.Message });
        }
    }

    [HttpPost]
    public JsonResult DeleteStudent(int id)
    {
        try
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_DeleteStudent", con);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id", id);
                con.Open();
                cmd.ExecuteNonQuery();
            }
            return Json(new { success = true, message = "Student deleted successfully." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Error: " + ex.Message });
        }
    }


    public ActionResult ExportStudentsToExcel()
    {
        List<Student> students = new List<Student>();


        using (SqlConnection con = new SqlConnection(connectionString))
        {
            SqlCommand cmd = new SqlCommand("sp_GetAllStudents", con);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            con.Open();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                students.Add(new Student
                {
                    Id = Convert.ToInt32(rdr["Id"]),
                    Name = rdr["Name"].ToString(),
                    Age = Convert.ToInt32(rdr["Age"]),
                    Email = rdr["Email"].ToString()
                });
            }
        }


        IWorkbook workbook = new XSSFWorkbook();
        ISheet sheet = workbook.CreateSheet("Students");


        ICellStyle headerStyle = workbook.CreateCellStyle();
        headerStyle.FillForegroundColor = IndexedColors.LightBlue.Index;
        headerStyle.FillPattern = FillPattern.SolidForeground;
        headerStyle.Alignment = HorizontalAlignment.Center;
        headerStyle.VerticalAlignment = VerticalAlignment.Center;


        IFont headerFont = workbook.CreateFont();
        headerFont.IsBold = true;
        headerFont.Color = IndexedColors.White.Index;
        headerStyle.SetFont(headerFont);


        ICellStyle cellStyle = workbook.CreateCellStyle();
        cellStyle.BorderBottom = BorderStyle.Thin;
        cellStyle.BorderTop = BorderStyle.Thin;
        cellStyle.BorderLeft = BorderStyle.Thin;
        cellStyle.BorderRight = BorderStyle.Thin;
        cellStyle.VerticalAlignment = VerticalAlignment.Center;


        IRow headerRow = sheet.CreateRow(0);
        string[] headers = { "ID", "Name", "Age", "Email" };

        for (int i = 0; i < headers.Length; i++)
        {
            ICell cell = headerRow.CreateCell(i);
            cell.SetCellValue(headers[i]);
            cell.CellStyle = headerStyle;
        }


        for (int i = 0; i < students.Count; i++)
        {
            IRow row = sheet.CreateRow(i + 1);
            row.CreateCell(0).SetCellValue(students[i].Id);
            row.CreateCell(1).SetCellValue(students[i].Name);
            row.CreateCell(2).SetCellValue(students[i].Age);
            row.CreateCell(3).SetCellValue(students[i].Email);

            for (int j = 0; j < 4; j++)
            {
                row.GetCell(j).CellStyle = cellStyle;
            }
        }


        for (int col = 0; col < headers.Length; col++)
        {
            sheet.AutoSizeColumn(col);
        }


        using (var exportData = new MemoryStream())
        {
            workbook.Write(exportData);
            string fileName = "Students.xlsx";
            return File(exportData.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }


    [HttpGet]
    public ActionResult UploadExcel()
    {
        return View();
    }

    [HttpPost]
    public ActionResult UploadExcel(HttpPostedFileBase excelFile)
    {
        if (excelFile == null || excelFile.ContentLength == 0)
        {
            TempData["Error"] = "Please select a file to upload";
            return View();
        }

        try
        {
            // Create DataTable to store Excel data
            DataTable dt = new DataTable();
            ISheet sheet;

            using (var stream = excelFile.InputStream)
            {
                // Create the Excel workbook
                if (excelFile.FileName.EndsWith(".xls"))
                {
                    HSSFWorkbook workbook = new HSSFWorkbook(stream);
                    sheet = workbook.GetSheetAt(0);
                }
                else
                {
                    XSSFWorkbook workbook = new XSSFWorkbook(stream);
                    sheet = workbook.GetSheetAt(0);
                }

                // Get the first row to create columns
                IRow headerRow = sheet.GetRow(0);
                int cellCount = headerRow.LastCellNum;

                // Create columns in DataTable
                for (int i = 0; i < cellCount; i++)
                {
                    dt.Columns.Add(headerRow.GetCell(i)?.ToString() ?? $"Column{i + 1}");
                }

                // Read data rows
                for (int i = 1; i <= sheet.LastRowNum; i++)
                {
                    IRow row = sheet.GetRow(i);
                    if (row == null) continue;

                    DataRow dataRow = dt.NewRow();
                    for (int j = 0; j < cellCount; j++)
                    {
                        ICell cell = row.GetCell(j);
                        if (cell != null)
                        {
                            switch (cell.CellType)
                            {
                                case CellType.Numeric:
                                    if (DateUtil.IsCellDateFormatted(cell))
                                        dataRow[j] = cell.DateCellValue;
                                    else
                                        dataRow[j] = cell.NumericCellValue;
                                    break;
                                case CellType.String:
                                    dataRow[j] = cell.StringCellValue;
                                    break;
                                case CellType.Boolean:
                                    dataRow[j] = cell.BooleanCellValue;
                                    break;
                                default:
                                    dataRow[j] = cell.ToString();
                                    break;
                            }
                        }
                    }
                    dt.Rows.Add(dataRow);
                }
            }

            // Convert DataTable to JSON
            string jsonData = JsonConvert.SerializeObject(dt);

            // Store data in database
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_ImportExcelData", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@JsonData", jsonData);
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();

                // merge ExcelData into Students
                SqlCommand mergeCmd = new SqlCommand("sp_MergeExcelData", con);
                mergeCmd.CommandType = CommandType.StoredProcedure;
                con.Open();
                mergeCmd.ExecuteNonQuery();
            }

            TempData["Success"] = "File uploaded and processed successfully!";
            return RedirectToAction("ExcelData");
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Error: " + ex.Message;
            return View();
        }
    }

    public ActionResult ExcelData()
    {
        DataTable dt = new DataTable();
        using (SqlConnection con = new SqlConnection(connectionString))
        {
            SqlCommand cmd = new SqlCommand("sp_GetExcelData", con);
            cmd.CommandType = CommandType.StoredProcedure;
            con.Open();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(dt);
        }
        return View(dt);
    }

    public ActionResult Logout()
    {
        Session.Clear();
        Session.Abandon();
        return RedirectToAction("About");
    }
}