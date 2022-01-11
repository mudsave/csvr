package local.campas.ppgame.xlsx2json;

import java.io.FileOutputStream;
import java.io.InputStream;
import java.io.OutputStreamWriter;
import java.io.Writer;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.Arrays;
import java.util.List;
import java.util.Map;
import java.util.Optional;
import java.io.File; 

import org.apache.poi.ss.usermodel.Cell;
import org.apache.poi.ss.usermodel.Row;
import org.apache.poi.ss.usermodel.Sheet;
import org.apache.poi.xssf.usermodel.XSSFSheet;
import org.apache.poi.xssf.usermodel.XSSFWorkbook;
import org.json.JSONObject;

import com.google.common.base.Objects;
import com.google.common.base.Strings;
import com.google.common.collect.ArrayListMultimap;
import com.google.common.collect.ImmutableList;
import com.google.common.collect.Iterables;
import com.google.common.collect.Maps;
import com.google.common.collect.Multimap;

public class Application {
    static Map<String, JSONObject> jsonMap = Maps.newHashMap();
    static Map<String, String> valueMap = Maps.newHashMap();
    static Map<String, Map<String, String>> keyPositionMap = Maps.newHashMap();
    static Multimap<String, String> errorMsgMap = ArrayListMultimap.create();
    static String currentFile = "";

    static boolean debug;

    public static void main(String[] args) throws Exception {
        String outPath;
        String[] docPaths;
        if (args.length < 3) {
            debug = true;
            outPath = "../output";
            docPaths = new String[]{".."};
        } else {
            debug = Boolean.parseBoolean(args[0]);
            outPath = args[1];
            docPaths = Arrays.copyOfRange(args, 2, args.length);
        }

        for (String docPath : docPaths) {
            Path path = Paths.get(docPath);
            if (Files.exists(path)) {
                Files.walk(Paths.get(docPath))
                     .filter(p -> !p.getFileName().toString().startsWith("~"))
                     .filter(p -> p.getFileName().toString().endsWith(".xlsx"))
                     .forEach(Application::readFile);
            } else {
                System.out.println("警告：无法访问路径 " + path.toString());
            }
        }

        if (!errorMsgMap.isEmpty()) {
            errorMsgMap.asMap().forEach((k, vList) -> {
                System.err.println("==== " + k + " ====");
                vList.stream().limit(10).forEach(System.err::println);
                int omittedCount = vList.size() - 10;
                if (omittedCount > 0) {
                    System.err.println("(" + omittedCount + " 条已省略)");
                }
                System.err.println();
            });
            System.err.println("=====================================\n共 " + errorMsgMap.size() + " 条错误");
            System.exit(1);
        }

        jsonMap.forEach((k, v) -> write(outPath + "/" + k + ".json", v));
        System.out.println("完成。共 " + jsonMap.size() + " 个 JSON 文件。");
    }

    private static void write(final String filePath, final JSONObject jsonObject) {
        try {
        	File file = new File(filePath);
        	if(!file.getParentFile().exists()) {  
        		//如果目标文件所在的目录不存在，则创建父目录
        		if(!file.getParentFile().mkdirs()) {
        			return; 
        		}
        	}
        	
            Writer writer = new OutputStreamWriter(new FileOutputStream(filePath, false), "utf8");
            writer.write(jsonObject.toString(4));
            writer.close();
        } catch (Exception ex) {
            System.err.println("Error occurred while writting json file " + filePath + ": " + getMessage(ex));
            if (debug) {
                ex.printStackTrace();
            }
            System.exit(2);
        }
    }

    private static void readFile(final Path path) {
        currentFile = path.toString();
        try (InputStream is = Files.newInputStream(path)) {
            XSSFWorkbook wb = new XSSFWorkbook(is);

            XSSFSheet names = wb.getSheet("代对表=");
            if (names == null) {
                return;
            }

            valueMap.clear();
            for (Row row : names) {
                for (Cell cell : row) {
                    if (cell.getColumnIndex() == 0) {
                        continue;
                    }
                    String[] valuePair = XlsxUtils.getStringCellValue(cell).split(":");
                    if (valuePair.length != 2) {
                        continue;
                    }
                    valueMap.put(valuePair[0], valuePair[1]);
                }
            }

            for (Row row : names) {
                Cell cell = row.getCell(0);
                String[] namePair = XlsxUtils.getStringCellValue(cell).split(":");
                if (namePair.length != 2) {
                    continue;
                }
                String sheetName = "@" + namePair[0], objectName = namePair[1];
                Sheet sheet = wb.getSheet(sheetName);
                if (sheet == null) {
                    error("找不到代对表中指定的工作表 " + sheetName);
                    continue;
                }

                readSheet(sheet, objectName, getOrCreate(jsonMap, objectName));
            }
        } catch (Exception ex) {
            error(getMessage(ex));
            if (debug) {
                ex.printStackTrace();
            }
        }
    }

    private static void readSheet(final Sheet sheet, final String jsonName, final JSONObject jsonObject) {
        Optional<CellMeta> cellMetaOptional = findKey(sheet);
        if (!cellMetaOptional.isPresent()) {
            error(sheet, "找不到 ID 列");
            return;
        }

        CellMeta keyCellMeta = cellMetaOptional.get();
        List<CellMeta> cellMetaList = getMeta(sheet);
        for (Row row : Iterables.skip(sheet, 2)) {
            Cell keyCell = row.getCell(keyCellMeta.idx);
            if (keyCell == null || keyCell.getCellType() == Cell.CELL_TYPE_BLANK) {
                return;
            }
            String key = getConvertedValue(keyCell, keyCellMeta).toString();
            if (jsonObject.has(key)) {
                error(keyCell,
                      "ID " + key + " 已被使用: "
                              + keyPositionMap.get(jsonName).get(key));
                continue;
            }
            keyPositionMap.get(jsonName).put(key, currentFile + ":" + XlsxUtils.getPositionString(keyCell));
            jsonObject.put(key, readRow(row, cellMetaList));
        }
    }

    private static JSONObject readRow(final Row row, final List<CellMeta> cellMetaList) {
        JSONObject jsonObject = new JSONObject();
        for (CellMeta cellMeta : cellMetaList) {
            Cell cell = row.getCell(cellMeta.idx);
            jsonObject.put(cellMeta.key, getConvertedValue(cell, cellMeta));
        }
        return jsonObject;
    }

    private static Optional<CellMeta> findKey(final Sheet sheet) {
        for (Cell cell : sheet.getRow(0)) {
            String value = cell.getStringCellValue();
            if (!Strings.isNullOrEmpty(value)) {
                if (value.substring(value.indexOf("[") + 1, value.indexOf("]")).contains("!")) {
                    return Optional.of(new CellMeta(cell.getColumnIndex(), value));
                }
            }
        }
        return Optional.empty();
    }

    private static List<CellMeta> getMeta(final Sheet sheet) {
        final ImmutableList.Builder<CellMeta> builder = ImmutableList.builder();
        for (Cell cell : sheet.getRow(0)) {
            String value = cell.getStringCellValue();
            if (!Strings.isNullOrEmpty(value)) {
                try {
                    builder.add(new CellMeta(cell.getColumnIndex(), value));
                } catch (Exception ex) {
                    error(cell, "构建JSON元数据失败：" + getMessage(ex));
                }
            }
        }
        return builder.build();
    }

    private static JSONObject getOrCreate(final Map<String, JSONObject> jsonMap, final String objectName) {
        if (jsonMap.containsKey(objectName)) {
            return jsonMap.get(objectName);
        }

        JSONObject json = new JSONObject();
        jsonMap.put(objectName, json);
        keyPositionMap.put(objectName, Maps.newHashMap());
        return json;
    }

    private static String getMessage(Exception ex) {
        return Objects.firstNonNull(ex.getMessage(), ex.getClass().getName());
    }

    private static Object getConvertedValue(Cell cell, CellMeta cellMeta) {
        String stringValue = XlsxUtils.getStringCellValue(cell);
        if (Strings.isNullOrEmpty(stringValue)) {
            return cellMeta.converter.defaultValue();
        }

        if (cellMeta.valueMapped) {
            if (!valueMap.containsKey(stringValue)) {
                error(cell, "代对表中找不到需要映射的值 `" + stringValue + "`");
                return 0;
            }
            stringValue = valueMap.get(stringValue);
        }

        try {
            return cellMeta.converter.convert(stringValue);
        } catch (Exception ex) {
            error(cell, "指定的类型 " + cellMeta.converter
                    + " 无法解析值 `" + stringValue + "`: " + getMessage(ex));
            return 0;
        }
    }

    private static void error(String msg) {
        errorMsgMap.put(currentFile, msg);
    }

    private static void error(Sheet sheet, String msg) {
        error(XlsxUtils.getPositionString(sheet) + ": " + msg);
    }

    private static void error(Row row, String msg) {
        error(XlsxUtils.getPositionString(row) + ": " + msg);
    }


    private static void error(Cell cell, String msg) {
        error(XlsxUtils.getPositionString(cell) + ": " + msg);
    }
}
