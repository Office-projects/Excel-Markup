﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Office = Microsoft.Office.Core;
using Excel = Microsoft.Office.Interop.Excel;

namespace Markup.Scripts
{
    [ComVisible(true)]
    public class Ribbon : Office.IRibbonExtensibility
    {
        private Office.IRibbonUI ribbon;
        public static Ribbon ribbonref;
        public static int LineNbr;

		#region | Task Panes |

		public TaskPane.Settings mySettings;
        public Microsoft.Office.Tools.CustomTaskPane myTaskPaneSettings;

		#endregion

        #region | Ribbon Events |

		public Ribbon()
        {
        }

        public string GetCustomUI(string ribbonID)
        {
            return GetResourceText("Markup.Ribbon.xml");
        }

		private static string GetResourceText(string resourceName)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            string[] resourceNames = asm.GetManifestResourceNames();
            for (int i = 0; i < resourceNames.Length; ++i)
            {
                if (string.Compare(resourceName, resourceNames[i], StringComparison.OrdinalIgnoreCase) == 0)
                {
                    using (StreamReader resourceReader = new StreamReader(asm.GetManifestResourceStream(resourceNames[i])))
                    {
                        if (resourceReader != null)
                        {
                            return resourceReader.ReadToEnd();
                        }
                    }
                }
            }
            return null;
        }

        public void Ribbon_Load(Office.IRibbonUI ribbonUI)
        {
            try
            {
                this.ribbon = ribbonUI;
                ThisAddIn.e_ribbon = ribbonUI;
                Properties.Settings.Default.Markup_LastShapeName = "";
                ErrorHandler.SetLogPath();
                ErrorHandler.CreateLogRecord();
                AssemblyInfo.SetAddRemoveProgramsIcon("ExcelAddin.ico");
                System.Globalization.CultureInfo enUS = new System.Globalization.CultureInfo("en-US");
                System.Threading.Thread.CurrentThread.CurrentCulture = enUS;
            }
            catch (Exception ex)
            {
                ErrorHandler.DisplayMessage(ex);
            }
        }

        public System.Drawing.Bitmap GetButtonImage(Office.IRibbonControl control)
        {
            try
            {
                switch (control.Id)
                {
                    case "grpRevision":
					case "btnRev":
                        return Properties.Resources.RevTri;
					case "grpMarkups":
					case "btnCloudAll":
                        return Properties.Resources.Cloud;
                    case "btnCloudHold":
                        return Properties.Resources.CloudHold;
                    case "btnCloudHatch":
                        return Properties.Resources.CloudHatch;
                    case "btnAreaHatch":
                        return Properties.Resources.Hatch;
                    case  "btnCloudPartLeft":
                        return Properties.Resources.CloudPartLeft;
                    case  "btnCloudPartRight":
                        return Properties.Resources.CloudPartRight;
                    case "btnCloudPartTop":
                        return Properties.Resources.CloudPartTop;
                    case "btnCloudPartBottom":
                        return Properties.Resources.CloudPartBottom;
                    default:
                        return null;
                }

            }
            catch (Exception ex)
            {
                ErrorHandler.DisplayMessage(ex);
                return null;

            }

        }

        public string GetLabelText(Office.IRibbonControl control)
        {
            try
            {
                switch (control.Id)
                {
                    case "tabMarkup":
                        if (Application.ProductVersion.Substring(0, 2) == "15") //for Excel 2013
                        {
                            return AssemblyInfo.Title.ToUpper();
                        }
                        else
                        {
                            return AssemblyInfo.Title;
                        }
                    case "txtCopyright":
                        return "© " + AssemblyInfo.Copyright;
                    case "txtDescription":
                        return AssemblyInfo.Title.Replace("&", "&&") + " " + AssemblyInfo.AssemblyVersion;
                    case "txtReleaseDate":
                        DateTime dteCreateDate = Properties.Settings.Default.App_ReleaseDate;
                        return dteCreateDate.ToString("dd-MMM-yyyy hh:mm tt");
					case "txtRevisionCharacter":
						return Properties.Settings.Default.Markup_TriangleRevisionCharacter;
					default:
                        return string.Empty;
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.DisplayMessage(ex);
                return string.Empty;
            }
        }

        public int GetItemCount(Office.IRibbonControl control)
        {
            try
            {
                switch (control.Id)
                {
                    case "drpColorType":
                        return 4;
                    default:
                        return 0;
                }

            }
            catch (Exception ex)
            {
                ErrorHandler.DisplayMessage(ex);
                return 0;

            }

        }

        public string GetItemLabel(Office.IRibbonControl control, int index)
        {
            try
            {
                switch (control.Id)
                {
                    case "drpColorType":
                        switch (index)
                        {
                            case 0:
                                return "BLACK";
                            case 1:
                                return "BLUE";
                            case 2:
                                return "RED";
                            case 3:
                                return "GREEN";
                            default:
                                return string.Empty;
                        }
                    default:
                        return string.Empty;
                }

            }
            catch (Exception ex)
            {
                ErrorHandler.DisplayMessage(ex);
                return string.Empty;

            }

        }

        public string GetSelectedItemID(Office.IRibbonControl control)
        {
            try
            {
                Properties.Settings.Default.Markup_ShapeLineColor = System.Drawing.Color.Black;
                return control.Id; 

            }
            catch (Exception ex)
            {
                ErrorHandler.DisplayMessage(ex);
                return control.Id;

            }

        }

        public bool GetEnabled(Office.IRibbonControl control)
        {
            try
            {
                switch (control.Id)
                {
                    case "btnRev":
                    case "btnCloudAll":
                    case "btnCloudHold":
                    case "btnCloudHatch":
                    case "btnAreaHatch":
                    case "btnCloudPartLeft":
                    case "btnCloudPartRight":
                    case "btnCloudPartTop":
                    case "btnCloudPartBottom":
                        return ErrorHandler.IsEnabled(false);
                    default:
                        return false;
                }

            }
            catch (Exception ex)
            {
                ErrorHandler.DisplayMessage(ex);
                return false;

            }

        }

        public void OnAction(Office.IRibbonControl control)
        {
            try
            {
                LineNbr = 0;
                switch (control.Id)
                {
                    case "btnSelectColor":
                        SelectLineColor();
                        break;
					case "btnUpdateColor":
						UpdateLineColor();
						break;
					case "btnRev":
                        CreateRevisionTriangle();
                        break;
                    case "btnCloudAll":
                        CreateCloudPart("ALL");
                        break;
                    case "btnCloudHold":
                        CreateCloudHold();
                        break;
                    case "btnCloudHatch":
                        CreateCloudHatch();
                        break;
                    case "btnAreaHatch":
                        CreateAreaHatching();
                        break;
                    case "btnCloudPartLeft":
                        CreateCloudPart("L");
                        break;
                    case "btnCloudPartRight":
                        CreateCloudPart("R");
                        break;
                    case "btnCloudPartTop":
                        CreateCloudPart("T");
                        break;
                    case "btnCloudPartBottom":
                        CreateCloudPart("B");
                        break;
                    case "btnRemoveLastShape":
                        RemoveLastShape();
                        break;
                    case "btnRemoveAllShapes":
                        RemoveAllShapes();
                        break;
                    case "btnSettings":
                        OpenSettings();
                        break;
                    case "btnOpenReadMe":
                        OpenReadMe();
                        break;
                    case "btnOpenNewIssue":
                        OpenNewIssue();
                        break;
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.DisplayMessage(ex);
            }

        }

        public void OnAction_Dropdown(Office.IRibbonControl control, string itemId, int index)
        {
            try
            {
                switch (control.Id)
                {
                    case "drpColorType":
                        switch (index)
                        {
                            case 0:
                                Properties.Settings.Default.Markup_ShapeLineColor = System.Drawing.Color.Black;
                                break;
                            case 1:
                                Properties.Settings.Default.Markup_ShapeLineColor = System.Drawing.Color.Blue;
                                break;
                            case 2:
                                Properties.Settings.Default.Markup_ShapeLineColor = System.Drawing.Color.Red;
                                break;
                            case 3:
                                Properties.Settings.Default.Markup_ShapeLineColor = System.Drawing.Color.Green;
                                break;
                            default:
                                Properties.Settings.Default.Markup_ShapeLineColor = System.Drawing.Color.Black;
                                break;
                        }
                        break;
                    default:
                        break;
                }

            }
            catch (Exception ex)
            {
                ErrorHandler.DisplayMessage(ex);
                Properties.Settings.Default.Markup_ShapeLineColor = System.Drawing.Color.Black;

            }

        }

		public void OnChange(Office.IRibbonControl control, ref string text)
		{
			try
			{
				switch (control.Id)
				{
					case "txtRevisionCharacter":
						{
							Properties.Settings.Default.Markup_TriangleRevisionCharacter = text;
							break;
						}
				}
			}

			catch (Exception ex)
			{
				ErrorHandler.DisplayMessage(ex);
			}
		}

		#endregion

		#region | Ribbon Buttons |

		public void CreateRevisionTriangle()
        {
            Excel.Shape shpTriangle = null;
            Excel.Shape txtTriangle = null;
            Excel.ShapeRange shapeRange = null;
            try
            {
                if (ErrorHandler.IsEnabled(true) == false)
                {
                    return;
                }
                ErrorHandler.CreateLogRecord();
                string shapeName = AssemblyInfo.Title.ToLower();
                Single[,] triArray = new Single[4, 2];
                double x = 0;
                double y = Globals.ThisAddIn.Application.Selection.Top;
                double h = Globals.ThisAddIn.Application.Selection.RowHeight;
                double w = Convert.ToInt32(h * 2.2 / Math.Sqrt(3));
                double f = Globals.ThisAddIn.Application.Selection.Font.Size;
                double selWidth = Globals.ThisAddIn.Application.Selection.Width;
                double selLeft = Globals.ThisAddIn.Application.Selection.Left;
                double selHorAli = Globals.ThisAddIn.Application.Selection.HorizontalAlignment;
                double xlAliCntr = Convert.ToDouble(Excel.XlHAlign.xlHAlignCenter);

                if (selHorAli == xlAliCntr & selWidth > w)
                {
                    x = selLeft + (selWidth - w) / 2;
                }
                else
                {
                    x = selLeft;
                }

                triArray[0, 0] = Convert.ToSingle(x + w / 2);
                triArray[0, 1] = Convert.ToSingle(y);
                triArray[1, 0] = Convert.ToSingle(x);
                triArray[1, 1] = Convert.ToSingle(y + h);
                triArray[2, 0] = Convert.ToSingle(x + w);
                triArray[2, 1] = Convert.ToSingle(y + h);
                triArray[3, 0] = Convert.ToSingle(x + w / 2);
                triArray[3, 1] = Convert.ToSingle(y);

                shpTriangle = Globals.ThisAddIn.Application.ActiveSheet.Shapes.AddPolyline(triArray);
                shpTriangle.Name = shapeName + new string(' ', 1) + DateTime.Now.ToString(Properties.Settings.Default.Markup_ShapeDateFormat) + "1";
                shpTriangle.Line.Weight = Convert.ToSingle(1.5);

                //add a textbox to the triangle
                txtTriangle = Globals.ThisAddIn.Application.ActiveSheet.Shapes.AddTextbox(Microsoft.Office.Core.MsoTextOrientation.msoTextOrientationHorizontal, Convert.ToSingle(x), Convert.ToSingle(y + h * 0.2), Convert.ToSingle(w), Convert.ToSingle(h * 0.8));
                txtTriangle.Select();
                txtTriangle.TextEffect.Text = Properties.Settings.Default.Markup_TriangleRevisionCharacter;
                Globals.ThisAddIn.Application.Selection.Font.Color = Properties.Settings.Default.Markup_ShapeLineColor;
                Globals.ThisAddIn.Application.Selection.Font.Size = f;
                Globals.ThisAddIn.Application.Selection.Border.LineStyle = Excel.Constants.xlNone;
                Globals.ThisAddIn.Application.Selection.Interior.ColorIndex = Excel.Constants.xlNone;
                Globals.ThisAddIn.Application.Selection.Shadow = false;
                Globals.ThisAddIn.Application.Selection.RoundedCorners = false;
                txtTriangle.TextFrame.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                txtTriangle.TextFrame.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                txtTriangle.TextFrame.AutoSize = true;
                txtTriangle.Name = shapeName + new string(' ', 1) + DateTime.Now.ToString(Properties.Settings.Default.Markup_ShapeDateFormat) + "2";

                //group both shapes together
                object[] shapes = { shpTriangle.Name, txtTriangle.Name };
                shapeRange = Globals.ThisAddIn.Application.ActiveSheet.Shapes.Range(shapes);
                shapeRange.Group();
                shapeRange.Name = shapeName + new string(' ', 1) + DateTime.Now.ToString(Properties.Settings.Default.Markup_ShapeDateFormat) + "3";
                shpTriangle.Select();
                Globals.ThisAddIn.Application.Selection.Interior.Pattern = Excel.XlPattern.xlPatternNone;
                SetLineColor();
                Globals.ThisAddIn.Application.ActiveCell.Select();
                Properties.Settings.Default.Markup_LastShapeName = shapeRange.Name;
            }
            catch (Exception ex)
            {
                ErrorHandler.DisplayMessage(ex);

            }
            finally
            {
                if (shapeRange != null) Marshal.ReleaseComObject(shapeRange);
                if (txtTriangle != null) Marshal.ReleaseComObject(txtTriangle);
                if (shpTriangle != null) Marshal.ReleaseComObject(shpTriangle);
            }
        }

        public void CreateCloudHold()
        {
            Excel.Shape cloudLineTop = null;
            Excel.Shape cloudLineRight = null;
            Excel.Shape cloudLineBottom = null;
            Excel.Shape cloudLineLeft = null;
            Excel.ShapeRange shapeRange = null;
            try
            {
                if (ErrorHandler.IsEnabled(true) == false)
                {
                    return;
                }
                ErrorHandler.CreateLogRecord();
                string shapeName = AssemblyInfo.Title.ToLower();
                double x = Globals.ThisAddIn.Application.Selection.Left;
                double y = Globals.ThisAddIn.Application.Selection.Top;
                double h = Globals.ThisAddIn.Application.Selection.Height;
                double w = Globals.ThisAddIn.Application.Selection.Width;
                double off = 7.5;
                x = x - off / 2;
                w = w + off;
                y = y - off / 2;
                h = h + off;
                cloudLineTop = CreateCloudLine(x, y, x + w, y);
                cloudLineRight = CreateCloudLine(x + w, y, x + w, y + h);
                cloudLineBottom = CreateCloudLine(x + w, y + h, x, y + h);
                cloudLineLeft = CreateCloudLine(x, y + h, x, y);
                if (cloudLineBottom != null && cloudLineTop != null && cloudLineLeft != null && cloudLineRight != null) // only if there are no errors in returning an Excel shape
                {
                    object[] shapes = { cloudLineBottom.Name, cloudLineTop.Name, cloudLineLeft.Name, cloudLineRight.Name };
                    shapeRange = Globals.ThisAddIn.Application.ActiveSheet.Shapes.Range(shapes);
                    shapeRange.Group();
                    shapeRange.Name = shapeName + new string(' ', 1) + DateTime.Now.ToString(Properties.Settings.Default.Markup_ShapeDateFormat);
                    Properties.Settings.Default.Markup_LastShapeName = shapeRange.Name;
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.DisplayMessage(ex);

            }
            finally
            {
                if (cloudLineTop != null) Marshal.ReleaseComObject(cloudLineTop);
                if (cloudLineRight != null) Marshal.ReleaseComObject(cloudLineRight);
                if (cloudLineBottom != null) Marshal.ReleaseComObject(cloudLineBottom);
                if (cloudLineLeft != null) Marshal.ReleaseComObject(cloudLineLeft);
                if (shapeRange != null) Marshal.ReleaseComObject(shapeRange);
            }
        }

        public void CreateCloudHatch()
        {
            Excel.Shape cloudPart = null;
            Excel.Shape hatchArea = null;
            Excel.ShapeRange shapeRange = null;
            try
            {
                if (ErrorHandler.IsEnabled(true) == false)
                {
                    return;
                }
                ErrorHandler.CreateLogRecord();
                string shapeName = AssemblyInfo.Title.ToLower();
                double x = Globals.ThisAddIn.Application.Selection.Left;
                double y = Globals.ThisAddIn.Application.Selection.Top;
                double h = Globals.ThisAddIn.Application.Selection.Height;
                double w = Globals.ThisAddIn.Application.Selection.Width;
                cloudPart = CreateCloudPart("ALL");
                hatchArea = CreateHatchArea(x, y, h, w);
                if (cloudPart != null && hatchArea != null) 
                {
                    object[] shapes = { cloudPart.Name, hatchArea.Name };
                    shapeRange = Globals.ThisAddIn.Application.ActiveSheet.Shapes.Range(shapes); //.Group();
                    shapeRange.Group();
                    shapeRange.Name = shapeName + new string(' ', 1) + DateTime.Now.ToString(Properties.Settings.Default.Markup_ShapeDateFormat);
                    Properties.Settings.Default.Markup_LastShapeName = shapeRange.Name;
                    Marshal.FinalReleaseComObject(cloudPart);
                    Marshal.FinalReleaseComObject(hatchArea);
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.DisplayMessage(ex);

            }
            finally
            {
                if (cloudPart != null) Marshal.ReleaseComObject(cloudPart);
                if (hatchArea != null) Marshal.ReleaseComObject(hatchArea);
                if (shapeRange != null) Marshal.ReleaseComObject(shapeRange);
            }
        }

        public void CreateAreaHatching()
        {
            try
            {
                if (ErrorHandler.IsEnabled(true) == false)
                {
                    return;
                }
                ErrorHandler.CreateLogRecord();
                int selectAreaCnt = Globals.ThisAddIn.Application.Selection.Areas.Count;
                if (selectAreaCnt > 1)
                {
                    foreach (Excel.Range singleArea in Globals.ThisAddIn.Application.Selection.Areas)
                    {
                        CreateHatchArea(singleArea.Left, singleArea.Top, singleArea.Height, singleArea.Width);
                    }
                }
                else
                {
                    CreateHatchArea(Globals.ThisAddIn.Application.Selection.Left, Globals.ThisAddIn.Application.Selection.Top, Globals.ThisAddIn.Application.Selection.Height, Globals.ThisAddIn.Application.Selection.Width);
                }

            }
            catch (Exception ex)
            {
                ErrorHandler.DisplayMessage(ex);

            }
        }

        public void SelectLineColor()
        {
            ErrorHandler.CreateLogRecord();
            Properties.Settings.Default.Markup_ShapeLineColor = SelectColor();
        }

        public void RemoveLastShape()
        {
            Excel.Worksheet xlWorkSheet = null;
            try
            {
                ErrorHandler.CreateLogRecord();
                if (ErrorHandler.IsActiveDocument(true) == false)
                {
                    return;
                }
                DialogResult dialogResult = MessageBox.Show("Are you sure you would like to delete the last shape that has been created?", "Delete Last Shape?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.No)
                {
                    return;
                }
                else if (dialogResult == DialogResult.Yes)
                {
                    xlWorkSheet = Globals.ThisAddIn.Application.ActiveSheet;
                    string strShapeName = Properties.Settings.Default.Markup_LastShapeName;
                    foreach (Excel.Shape shp in xlWorkSheet.Shapes)
                    {
                        if (shp.Name == strShapeName)
                        {
                            shp.Delete();
                        }
                        if (shp != null) Marshal.ReleaseComObject(shp);
                    }
                    if (xlWorkSheet != null) Marshal.ReleaseComObject(xlWorkSheet);
                }
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                MessageBox.Show("You are currently editing a cell." + Environment.NewLine + Environment.NewLine + "Please finish editing and press return.", "No action taken.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

            }
            catch (Exception ex)
            {
                ErrorHandler.DisplayMessage(ex);

            }
            finally
            {
                if (xlWorkSheet != null) Marshal.ReleaseComObject(xlWorkSheet);
            }
        }

        public void RemoveAllShapes()
        {
            Excel.Worksheet xlWorkSheet = null;
            try
            {

                ErrorHandler.CreateLogRecord();
                if (ErrorHandler.IsActiveDocument(true) == false)
                {
                    return;
                }
                DialogResult dialogResult = MessageBox.Show("Are you sure you would like to delete all the shapes in the active worksheet?", "Delete All Shapes?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.No)
                {
                    return;
                }
                else if (dialogResult == DialogResult.Yes)
                {
                    xlWorkSheet = Globals.ThisAddIn.Application.ActiveSheet;
                    foreach (Excel.Shape shp in xlWorkSheet.Shapes)
                    {
                        if (shp.Type == Microsoft.Office.Core.MsoShapeType.msoGroup || shp.Type == Microsoft.Office.Core.MsoShapeType.msoLine || shp.Type == Microsoft.Office.Core.MsoShapeType.msoFreeform)
                        {
                            string s = shp.Name;
                            if (s.Contains(AssemblyInfo.Title.ToLower()))
                            {
                                shp.Delete();
                            }
                        }
                        if (shp != null) Marshal.ReleaseComObject(shp);
                    }
                    if (xlWorkSheet != null) Marshal.ReleaseComObject(xlWorkSheet);
                }
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                MessageBox.Show("You are currently editing a cell." + Environment.NewLine + Environment.NewLine + "Please finish editing and press return.", "No action taken.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

            }
            catch (Exception ex)
            {
                ErrorHandler.DisplayMessage(ex);

            }
            finally
            {
                if (xlWorkSheet != null) Marshal.ReleaseComObject(xlWorkSheet);
            }

        }

        public void OpenSettings()
        {
            try
            {
                if (myTaskPaneSettings != null)
                {
                    if (myTaskPaneSettings.Visible == true)
                    {
                        myTaskPaneSettings.Visible = false;
                    }
                    else
                    {
                        myTaskPaneSettings.Visible = true;
                    }
                }
                else
                {
                    mySettings = new TaskPane.Settings();
                    myTaskPaneSettings = Globals.ThisAddIn.CustomTaskPanes.Add(mySettings, "Settings for " + Scripts.AssemblyInfo.Title);
                    myTaskPaneSettings.DockPosition = Office.MsoCTPDockPosition.msoCTPDockPositionRight;
                    myTaskPaneSettings.DockPositionRestrict = Office.MsoCTPDockPositionRestrict.msoCTPDockPositionRestrictNoChange;
                    myTaskPaneSettings.Width = 675;
                    myTaskPaneSettings.Visible = true;
                }

            }
            catch (Exception ex)
            {
                ErrorHandler.DisplayMessage(ex);
            }
        }

        public void OpenReadMe()
        {
            ErrorHandler.CreateLogRecord();
            System.Diagnostics.Process.Start(Properties.Settings.Default.App_PathReadMe);

        }

        public void OpenNewIssue()
        {
            ErrorHandler.CreateLogRecord();
            System.Diagnostics.Process.Start(Properties.Settings.Default.App_PathNewIssue);

        }

		#endregion

		#region | Subroutines |

		public Excel.Shape CreateArc(double x1, double y1, double x2, double y2, double length)
        {
            Excel.Shape cloudArc = null;
            try
            {
                string shapeName = AssemblyInfo.Title.ToLower();
                int i = 0;
                double angle = 60;
                double segments = angle / 10;
                float[,] arcArray = new float[Convert.ToInt32(segments) + 1, 2];
                double theta = angle * Math.PI / 180;
                double xm = (x1 + x2) / 2;
                double ym = (y1 + y2) / 2;
                double xd = (x2 - x1);
                double yd = (y2 - y1);
                double d = Math.Sqrt(xd * xd + yd * yd);
                double r = d / 2 / Math.Sin(theta / 2);
                double xc = xm + yd / (2 * Math.Tan(theta / 2));
                double yc = ym - xd / (2 * Math.Tan(theta / 2));
                double dtheta = theta / segments;
                arcArray[0, 0] = Convert.ToSingle(x1);
                arcArray[0, 1] = Convert.ToSingle(y1);
                double a = Math.Atan2(y1 - yc, x1 - xc) - dtheta;
                for (i = 1; i <= Convert.ToInt32(segments) - 1; i++)
                {
                    arcArray[i, 0] = Convert.ToSingle(xc + r * Math.Cos(a));
                    arcArray[i, 1] = Convert.ToSingle(yc + r * Math.Sin(a));
                    a = a - dtheta;
                }
                arcArray[i, 0] = Convert.ToSingle(x2);
                arcArray[i, 1] = Convert.ToSingle(y2);
                cloudArc = Globals.ThisAddIn.Application.ActiveSheet.Shapes.AddPolyline(arcArray);
                cloudArc.Select();
                cloudArc.Name = shapeName + new string(' ', 1) + DateTime.Now.ToString(Properties.Settings.Default.Markup_ShapeDateFormat) + LineNbr.ToString();
                LineNbr += 1;
                Globals.ThisAddIn.Application.Selection.Interior.Pattern = Excel.Constants.xlNone;
                SetLineColor();
                return cloudArc;

            }
            catch (Exception ex)
            {
                ErrorHandler.DisplayMessage(ex);
                return null;

            }
        }

        public Excel.Shape CreateCloudLine(double x1, double y1, double x2, double y2)
        {
            Excel.Shape cloudArc = null;
            Excel.Shape cloudLine = null;
            Excel.ShapeRange shapeRange = null;
            try
            {
                string shapeName = AssemblyInfo.Title.ToLower();
                double length = Properties.Settings.Default.Markup_ShapeLineSpacing;
                int i = 0;
                double x = 0;
                double y = 0;
                double dx = x2 - x1;
                double dy = y2 - y1;
                double d = Math.Sqrt(dx * dx + dy * dy);
                double segments = Math.Ceiling(d / length);
                if (segments < 2)
                    segments = 2;
                double deltax = (dx / segments);
                double deltay = (dy / segments);
                double xp = x1;
                double yp = y1;
                object[] shapes = new object[Convert.ToInt32(segments)];
                for (i = 1; i <= Convert.ToInt32(segments); i++)
                {
                    x = xp + deltax;
                    y = yp + deltay;
                    cloudArc = CreateArc(xp, yp, x, y, length);
                    shapes[i - 1] = cloudArc.Name; 
                    xp = x;
                    yp = y;
                }
                shapeRange = Globals.ThisAddIn.Application.ActiveSheet.Shapes.Range(shapes); //.Group();
                shapeRange.Group();
                shapeRange.Name = shapeName + new string(' ', 1)  + DateTime.Now.ToString(Properties.Settings.Default.Markup_ShapeDateFormat) + LineNbr.ToString();
                LineNbr += 1;
                cloudLine = Globals.ThisAddIn.Application.ActiveSheet.Shapes(shapeRange.Name);
                Properties.Settings.Default.Markup_LastShapeName = shapeRange.Name;
                return cloudLine;
            }
            catch (Exception ex)
            {
                ErrorHandler.DisplayMessage(ex, true);
                return null;

            }
        }

        public Excel.Shape CreateCloudPart(string cloudPart)
        {
            if (ErrorHandler.IsEnabled(true) == false)
            {
                return null;
            }
            Excel.Shape cloudLineBottom = null;
            Excel.Shape cloudLineTop = null;
            Excel.Shape cloudLineLeft = null;
            Excel.Shape cloudLineRight = null;
            Excel.Shape cloudLine = null;
            Excel.ShapeRange shapeRange = null;
            try
            {
                double x = Globals.ThisAddIn.Application.Selection.Left;
                double y = Globals.ThisAddIn.Application.Selection.Top;
                double h = Globals.ThisAddIn.Application.Selection.Height;
                double w = Globals.ThisAddIn.Application.Selection.Width;

                if (cloudPart == "B" | cloudPart == "ALL")
                {
                    cloudLineBottom = CreateCloudLine(x, y + h, x + w, y + h);
                }
                if (cloudPart == "T" | cloudPart == "ALL")
                {
                    cloudLineTop = CreateCloudLine(x + w, y, x, y);
                }
                if (cloudPart == "L" | cloudPart == "ALL")
                {
                    cloudLineLeft = CreateCloudLine(x, y, x, y + h);
                }
                if (cloudPart == "R" | cloudPart == "ALL")
                {
                    cloudLineRight = CreateCloudLine(x + w, y + h, x + w, y);
                }

                if (cloudPart == "ALL" && cloudLineBottom != null && cloudLineTop != null && cloudLineLeft != null && cloudLineRight != null)
                {
                    string shapeName = AssemblyInfo.Title.ToLower();
                    object[] shapes = { cloudLineBottom.Name, cloudLineTop.Name, cloudLineLeft.Name, cloudLineRight.Name };
                    shapeRange = Globals.ThisAddIn.Application.ActiveSheet.Shapes.Range(shapes); //.Group();
                    shapeRange.Group();
                    shapeRange.Name = shapeName + new string(' ', 1) + DateTime.Now.ToString(Properties.Settings.Default.Markup_ShapeDateFormat);
                    cloudLine = Globals.ThisAddIn.Application.ActiveSheet.Shapes(shapeRange.Name);
                    Properties.Settings.Default.Markup_LastShapeName = shapeRange.Name;
                    return cloudLine;
                }
                else
                {
                    return null;
                }

            }
            catch (Exception ex)
            {
                ErrorHandler.DisplayMessage(ex);
                return null;

            }
            finally
            {
                if (cloudLineBottom != null) Marshal.ReleaseComObject(cloudLineBottom);
                if (cloudLineTop != null) Marshal.ReleaseComObject(cloudLineTop);
                if (cloudLineLeft != null) Marshal.ReleaseComObject(cloudLineLeft);
                if (cloudLineRight != null) Marshal.ReleaseComObject(cloudLineRight);
                if (cloudLine != null) Marshal.ReleaseComObject(cloudLine);
                if (shapeRange != null) Marshal.ReleaseComObject(shapeRange);
            }

        }

        public Excel.Shape CreateHatchArea(double x, double y, double h, double w)
        {
            string shapeName = AssemblyInfo.Title.ToLower();
            double length = Properties.Settings.Default.Markup_ShapeLineSpacing;
			double xx1 = 0;
            double yy1 = 0;
            double xx2 = 0;
            double yy2 = 0;
            double x1 = x + y;
            double x2 = Math.Floor(x1 / length);
            double x3 = (x2 + 1) * length;
            double xDiff = x3 - x1;
            double xl = x;
            double xr = x + w;
            double xw = w;
            double yt = y;
            double yb = y + h;
            double yw = h;
            double xsp = x + xDiff;
            double ysp = yt + xDiff;
            Excel.Shape hatchLine1 = null;
            Excel.Shape hatchLine2 = null;
            Excel.Shape hatchArea = null;
            Excel.ShapeRange shapeRange = null;
            List<object> shapesList = new List<object>();
            try
            {
                if (xw >= yw)
                {
                    while (xsp < xr + yw)
                    {
                        if (xsp - xl < yb - yt)
                        {
                            xx1 = xsp;
                            yy1 = yt;
                            xx2 = xl;
                            yy2 = yt + (xx1 - xl);
                        }
                        else
                        {
                            if (xsp <= xr)
                            {
                                xx1 = xsp;
                                yy1 = yt;
                                xx2 = xx1 - yw;
                                yy2 = yb;
                            }
                            else
                            {
                                xx2 = xsp - yw;
                                yy2 = yb;
                                xx1 = xr;
                                yy1 = yb - (xr - xx2);
                            }
                        }
                        hatchLine1 = Globals.ThisAddIn.Application.ActiveSheet.Shapes.AddLine(Convert.ToSingle(xx1), Convert.ToSingle(yy1), Convert.ToSingle(xx2), Convert.ToSingle(yy2));
                        hatchLine1.Select();
                        hatchLine1.Name = shapeName + new string(' ', 1) + DateTime.Now.ToString(Properties.Settings.Default.Markup_ShapeDateFormat) + LineNbr.ToString();
                        shapesList.Add(hatchLine1.Name);
                        SetLineColor();
                        xsp = xsp + length;
                        LineNbr += 1;
                    }
                }
                else
                {
                    while (ysp < yb + xw)
                    {
                        if (ysp - yt < xw)
                        {
                            xx2 = xl;
                            yy2 = ysp;
                            xx1 = xl + (yy2 - yt);
                            yy1 = yt;
                        }
                        else
                        {
                            if (ysp <= yb)
                            {
                                xx2 = xl;
                                yy2 = ysp;
                                xx1 = xr;
                                yy1 = yy2 - xw;
                            }
                            else
                            {
                                xx1 = xr;
                                xx2 = xl + (ysp - yb);
                                yy2 = yb;
                                yy1 = yb - (xr - xx2);
                            }
                        }
                        hatchLine2 = Globals.ThisAddIn.Application.ActiveSheet.Shapes.AddLine(Convert.ToSingle(xx1), Convert.ToSingle(yy1), Convert.ToSingle(xx2), Convert.ToSingle(yy2));
                        hatchLine2.Select();
                        hatchLine2.Name = shapeName + new string(' ', 1) + DateTime.Now.ToString(Properties.Settings.Default.Markup_ShapeDateFormat) + LineNbr.ToString();
                        shapesList.Add(hatchLine2.Name);
                        SetLineColor();
                        ysp = ysp + length;
                        LineNbr += 1;
                    }
                }
                object[] shapes = shapesList.ToArray();
                shapeRange = Globals.ThisAddIn.Application.ActiveSheet.Shapes.Range(shapes);
                shapeRange.Group();
                shapeRange.Name = shapeName + new string(' ', 1) + DateTime.Now.ToString(Properties.Settings.Default.Markup_ShapeDateFormat);
                hatchArea = Globals.ThisAddIn.Application.ActiveSheet.Shapes(shapeRange.Name);
                Properties.Settings.Default.Markup_LastShapeName = shapeRange.Name;
                return hatchArea;
            }

            catch (System.Runtime.InteropServices.COMException ex)
            {
                ErrorHandler.DisplayMessage(ex, true);
                return null;

            }
            catch (Exception ex)
            {
                ErrorHandler.DisplayMessage(ex, true);
                return null;

            }
            finally
            {
                if (shapeRange != null) Marshal.FinalReleaseComObject(shapeRange);
                if (hatchLine1 != null) Marshal.FinalReleaseComObject(hatchLine1);
                if (hatchLine2 != null) Marshal.FinalReleaseComObject(hatchLine2);

            }
        }

        public void SetLineColor()
        {
            try
            {
                Globals.ThisAddIn.Application.Selection.ShapeRange.Line.Visible = true;
                Globals.ThisAddIn.Application.Selection.ShapeRange.Line.ForeColor.RGB = Properties.Settings.Default.Markup_ShapeLineColor;
            }
            catch (Exception ex)
            {
                ErrorHandler.DisplayMessage(ex);

            }

        }

        public System.Drawing.Color SelectColor()
        {
            try
            {
                ColorDialog colorDlg = new ColorDialog();
                if (colorDlg.ShowDialog() == DialogResult.OK)
                {
                    Properties.Settings.Default.Markup_ShapeLineColor = colorDlg.Color;
                }
                if (ErrorHandler.IsActiveSelection(false) == false)
                {
                    Globals.ThisAddIn.Application.Selection.ShapeRange.Line.ForeColor.RGB = Properties.Settings.Default.Markup_ShapeLineColor;
                }
                return Properties.Settings.Default.Markup_ShapeLineColor;
            }

            catch (Exception ex)
            {
                ErrorHandler.DisplayMessage(ex);
                return System.Drawing.Color.Black;

            }
        }

        public void InvalidateRibbon()
        {
            ribbon.Invalidate();
        }

		public void UpdateLineColor()
		{
			try
			{
				if (ErrorHandler.IsActiveSelection() == false)
				{
					Excel.ShapeRange shapeObjects = Globals.ThisAddIn.Application.Selection.ShapeRange;
					foreach (Excel.Shape shape in shapeObjects)
					{
						if (shape.Name.StartsWith("RevTri") && shape.Type != Microsoft.Office.Core.MsoShapeType.msoTextBox)
						{
							shape.Select();
							Globals.ThisAddIn.Application.Selection.ShapeRange.Line.ForeColor.RGB = Properties.Settings.Default.Markup_ShapeLineColor;
							Globals.ThisAddIn.Application.Selection.Font.Color = Properties.Settings.Default.Markup_ShapeLineColor;
						}
						else
						{
							if (shape.Type != Microsoft.Office.Core.MsoShapeType.msoTextBox)
							{
								SetLineColor();
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.DisplayMessage(ex);

			}
		}

        #endregion

    }
}