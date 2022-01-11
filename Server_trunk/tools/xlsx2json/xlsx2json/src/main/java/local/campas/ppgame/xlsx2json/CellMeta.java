package local.campas.ppgame.xlsx2json;

public class CellMeta {
    int idx;
    String key;
    boolean valueMapped;
    Converter converter;

    public CellMeta(final int index, final String metaString) {
        idx = index;
        key = metaString.substring(0, metaString.indexOf('['));
        valueMapped = metaString.substring(metaString.indexOf('[') + 1, metaString.indexOf(']')).contains("$");
        String converterName = metaString.substring(metaString.lastIndexOf('[') + 1, metaString.lastIndexOf(']'));
        converter = Converter.valueOf(converterName);
    }
}
