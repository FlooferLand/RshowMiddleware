namespace RshowMiddleware;

public static class Util {
    public static long ByteToMegabyte(long bytes) {
        return bytes / (1024 * 1024);
    }
}