using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ClosedXML.Excel;
using ShiftCaptain.Models;
using System.ComponentModel.DataAnnotations;

namespace ShiftCaptain.Infrastructure
{
    public class OutputManager
    {
        ShiftCaptainEntities db = new ShiftCaptainEntities();

        public XLWorkbook Export(int VersionId)
        {
            var rooms = db.RoomViews.Where(rv => rv.VersionId == VersionId);

            XLWorkbook workbook = new XLWorkbook();
            
            foreach (var room in rooms)
            {
                IXLWorksheet ws = workbook.AddWorksheet(room.Name);
                ws.Columns().Width = 6;
                ws.Columns().Style.Fill.BackgroundColor = XLColor.White;
                ws.Columns().Style.Font.FontSize = 20;
                ws.Columns("A").Width = 7;
                ws.Rows().Height = 40;
                ws.Rows().Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                
                var roomHours = db.RoomHours.Where(rh => rh.RoomInstanceId == room.RoomInstanceId);
                TimeSpan EarliestStartTime;
                TimeSpan LatestClose;
                getHourInfo(ws, roomHours, out EarliestStartTime, out LatestClose);
                int ColumnCounter = setHours(ws, 1, EarliestStartTime, LatestClose);
                
                int currentRow = 2;
                int columnOffset = (int)(EarliestStartTime.TotalHours * 2);//don't put in extra blank spots.
                foreach (var dayInfo in roomHours)
                {
                    var dayOffset = dayInfo.StartTime - EarliestStartTime;
                    var shifts = db.Shifts.Where(s => s.RoomId == room.RoomId && s.Day == dayInfo.Day && s.VersionId == VersionId);
                    int startRow = currentRow;
                    foreach (var shiftRow in GetRows(shifts.ToList()))
                    {
                        
                        foreach (var shift in shiftRow)
                        {
                            int StartCol = (int)(shift.StartTime.TotalHours * 2) - columnOffset + 2;
                            int LengthCol = (int)(shift.Duration * 2);
                            int row = currentRow;

                            ws.Cell(row, StartCol).Value = shift.User.NickName;
                            ws.Cell(row, StartCol).Style.Font.FontSize = 25;
                            if (LengthCol > 1)
                            {
                                ws.Range(row, StartCol, row, StartCol + LengthCol - 1).Merge();
                            }

                            ws.Cell(row, StartCol).Style.Fill.SetBackgroundColor(ClosedXML.Excel.XLColor.LightGreen);
                            SetBorders(ws, row, currentRow, StartCol, StartCol + LengthCol - 1, XLBorderStyleValues.Thin);
                            ws.Cell(row, StartCol).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                        }
                        currentRow++;
                    }
                    AddClosedShifts(ws, EarliestStartTime, LatestClose, dayInfo.StartTime, (double)dayInfo.Duration, startRow, currentRow);

                    ws.Cell(startRow, 1).Value = Enum.GetName(typeof(DayOfWeek), dayInfo.Day).Substring(0, 3);
                    ws.Range(startRow, 1, currentRow, 1).Merge();
                    ws.Cell(startRow, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    ws.Cell(startRow, 1).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                    SetBorders(ws, startRow, currentRow, 1, 1, XLBorderStyleValues.Thin);
                    currentRow++;

                }
                setHours(ws, currentRow, EarliestStartTime, LatestClose);//put it on bottom also
                for (int i = 3; i < currentRow; i++)
                {
                    for (int j = 4; j < ColumnCounter; j += 2)
                    {
                        if (!ws.Cell(i, j).IsMerged() && ws.Cell(i, j).IsEmpty())
                            ws.Cell(i, j).Style.Border.SetLeftBorder(XLBorderStyleValues.MediumDashed);
                    }
                }
                //if (Header != "")
                //    ws.PageSetup.Header.Center.AddText(Header).SetFontSize(30);

                //if (Footer != "")
                //    ws.PageSetup.Footer.Center.AddText(Footer).SetFontSize(30);
                ws.PageSetup.PrintAreas.Add(1, 1, currentRow, ColumnCounter);
                ws.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                ws.PageSetup.FitToPages(1, 1);

                ws.PageSetup.Margins.Top = 1;
                ws.PageSetup.Margins.Bottom = 1;
                ws.PageSetup.Margins.Left = 0.25;
                ws.PageSetup.Margins.Right = 0.25;

                ws.PageSetup.CenterHorizontally = true;
                ws.PageSetup.CenterVertically = true;
            }
            return workbook;
        }

        private Shift GetNextShift(TimeSpan currentTime, List<Shift> shifts)
        {
            var possibleShifts = shifts.Where(s => s.StartTime >= currentTime);
            if (possibleShifts.Count() > 0)
            {
                var MinStartTime = possibleShifts.Min(s => s.StartTime);
                return shifts.FirstOrDefault(s => s.StartTime == MinStartTime);
            }
            return null;
        }
        private List<List<Shift>> GetRows(List<Shift> shifts)
        {
            List<List<Shift>> rows = new List<List<Shift>>();
            if (shifts.Count() == 0)
            {
                return rows;
            }
            var row = new List<Shift>();
            rows.Add(row);
            bool assignedShift = true;
            var currentTime = TimeSpan.MinValue;
            while (assignedShift)
            {
                assignedShift = false;
                var shift = GetNextShift(currentTime, shifts);
                if (shift != null)
                {
                    assignedShift = true;
                    row.Add(shift);
                    shifts.Remove(shift);
                    currentTime = shift.StartTime.Add(TimeSpan.FromHours((double)shift.Duration));
                }
                else
                {
                    //no more shifts fit on this row.
                    if (row.Count() != 0)
                    {
                        assignedShift = true;
                        row = new List<Shift>();
                        rows.Add(row);
                        currentTime = TimeSpan.MinValue;
                    }
                }
            }
            return rows;
        }
        private void getHourInfo(IXLWorksheet ws, IQueryable<RoomHour> hours, out TimeSpan EarliestStartTime, out TimeSpan LatestClose)
        {
            EarliestStartTime = TimeSpan.MinValue;//doesn't matter
            LatestClose = TimeSpan.MinValue;//doesn't matter
            
            var start = TimeSpan.MaxValue;
            var end = TimeSpan.MinValue;
            foreach (var hour in hours)
            {
                if (hour.StartTime < start)
                {
                    start = hour.StartTime;
                }
                var endHour = hour.StartTime.Add(TimeSpan.FromHours((double)hour.Duration));
                if (endHour > end)
                {
                    end = endHour;
                }
            }
            EarliestStartTime = start;
            LatestClose = end;
        }

        private int setHours(IXLWorksheet ws, int row, TimeSpan start, TimeSpan end)
        {
            int ColumnCounter = 2;
            for (var i = start; i < end; i += new TimeSpan(0, 30, 0))
            {
                var cell = i;
                while (cell >= new TimeSpan(13, 0, 0))
                    cell -= new TimeSpan(12, 0, 0);
                if (cell.Minutes == 0)
                {
                    ws.Cell(row, ColumnCounter).Value = "'" + cell.ToString("hh\\:mm");
                    ws.Range(row, ColumnCounter, row, ColumnCounter + 1).Merge();
                    SetBorders(ws, row, row, ColumnCounter, ColumnCounter + 1, XLBorderStyleValues.Thin);
                    ws.Cell(row, ColumnCounter).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    ColumnCounter += 2;
                }
            }
            return ColumnCounter;
        }

        private void SetBorders(IXLWorksheet ws, int startRow, int endRow, int startColumn, int endColumn, XLBorderStyleValues border)
        {
            for (int j = startRow; j <= endRow; j++)
                for (int k = startColumn; k <= endColumn; k++)
                    ws.Cell(j, k).Style.Border.SetOutsideBorder(border);
        }

        private void AddClosedShifts(IXLWorksheet ws, TimeSpan EarliestStartTime, TimeSpan LatestOpen, TimeSpan OpenTime, Double OpenDuration, int row, int lastRow)
        {
            int columnOffset = (int)(EarliestStartTime.TotalHours * 2);
            int StartCol = 2;
            int LengthCol = (int)((OpenTime - EarliestStartTime).TotalHours * 2);
            if (LengthCol > 0)
            {
                AddCell(ws, StartCol, LengthCol, row, lastRow);
            }
            StartCol = (int)(LatestOpen.TotalHours * 2) - columnOffset + 2;
            LengthCol = (int)((LatestOpen - OpenTime.Add(TimeSpan.FromHours(OpenDuration))).TotalHours * 2);

            if (LengthCol > 0)
            {
                StartCol -= LengthCol;
                AddCell(ws, StartCol, LengthCol, row, lastRow);
            }            
        }
        private void AddCell(IXLWorksheet ws, int startCol, int lengthCol, int row, int lastRow)
        {
            ws.Cell(row, startCol).Style.Fill.SetBackgroundColor(ClosedXML.Excel.XLColor.Gray);
            if (lengthCol > 1)
            {
                ws.Range(row, startCol, lastRow, startCol + lengthCol - 1).Merge();
            }

            SetBorders(ws, row, lastRow, startCol, startCol + lengthCol - 1, XLBorderStyleValues.Thin);
        }
    }
}