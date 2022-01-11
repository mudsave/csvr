package local.campas.ppgame.xlsx2json;

import org.apache.poi.ss.usermodel.Cell;
import org.apache.poi.ss.usermodel.Row;
import org.apache.poi.ss.usermodel.Sheet;

public class XlsxUtils {
    public static int char2Index(char charIndex) {
        return charIndex - 'A';
    }

    public static char index2Char(int index) {
        return (char) ('A' + index);
    }

    public static String getStringCellValue(Cell cell) {
        if (cell == null) {
            return "";
        }
        switch (cell.getCellType()) {
            case Cell.CELL_TYPE_NUMERIC:
                double d = cell.getNumericCellValue();
                return (long) d == d ? "" + (long) d : "" + d;
            case Cell.CELL_TYPE_STRING:
                return cell.getStringCellValue().trim();
            case Cell.CELL_TYPE_BLANK:
                return "";
            case Cell.CELL_TYPE_FORMULA:
                switch (cell.getCachedFormulaResultType()) {
                    case Cell.CELL_TYPE_NUMERIC:
                        return String.valueOf(cell.getNumericCellValue());
                    case Cell.CELL_TYPE_STRING:
                        return cell.getRichStringCellValue().toString();
                }
            default:
                throw new IllegalStateException("Unsupported cell value type: " + cell.getCellType() + " . sheetName : " + getPositionString(cell));
        }
    }

    public static String getPositionString(Sheet sheet) {
        return sheet.getSheetName();
    }

    public static String getPositionString(Row row) {
        return row.getSheet().getSheetName() + ":" + (row.getRowNum() + 1);
    }

    public static String getPositionString(Cell cell) {
        return cell.getSheet().getSheetName() + ":" + index2Char(cell.getColumnIndex()) + (cell.getRowIndex() + 1);
    }
}
