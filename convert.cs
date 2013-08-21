using System;
using System.IO;
using System.Text;

class Converter {
    static void Main(string[] args) {
        if (args.Length != 4) {
            Console.WriteLine("convert input_file book_title output_directory line_count");
            return;
        }

        string input_file = args[0];

        string title = args[1];

        string output_directory = args[2];
        string oebps_directory = args[2] + Path.DirectorySeparatorChar + "OEBPS";
        string meta_directory = args[2] + Path.DirectorySeparatorChar + "META-INF";

        int separationLineCount = Convert.ToInt32(args[3]);

        Directory.CreateDirectory(output_directory);
        Directory.CreateDirectory(oebps_directory);
        Directory.CreateDirectory(meta_directory);

        // Create mimetype file
        using (StreamWriter sw = new StreamWriter(output_directory + Path.DirectorySeparatorChar + "mimetype")) {
            sw.Write("application/epub+zip");
        }

        // Create container.xml
        using (StreamWriter sw = new StreamWriter(meta_directory + Path.DirectorySeparatorChar + "container.xml", false, Encoding.GetEncoding("UTF-8"))) {
            sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sw.WriteLine("<container version=\"1.0\" xmlns=\"urn:oasis:names:tc:opendocument:xmlns:container\">");
            sw.WriteLine("    <rootfiles>");
            sw.WriteLine("        <rootfile full-path=\"OEBPS/content.opf\" media-type=\"application/oebps-package+xml\"/>");
            sw.WriteLine("    </rootfiles>");
            sw.WriteLine("</container>");
        }

        int fileCount = 0;
        using (StreamReader sr = new StreamReader(input_file, Encoding.GetEncoding("UTF-16"))) {
            int lineCount = 0;
            string line;
            bool ended = false;
            string output_file = null;
            while ((line = sr.ReadLine()) != null) {
                output_file = oebps_directory + Path.DirectorySeparatorChar + fileCount + ".xhtml";
                if (lineCount % separationLineCount == 0) {
                    using (StreamWriter sw = new StreamWriter(output_file, false, Encoding.GetEncoding("UTF-16"))) {
                        sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-16\" standalone=\"no\"?>");
                        sw.WriteLine("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\"");
                        sw.WriteLine("  \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">");
                        sw.WriteLine("<html xmlns=\"http://www.w3.org/1999/xhtml\">");
                        sw.WriteLine("<head>");
                        sw.WriteLine("<style type=\"text/css\">");
                        sw.WriteLine("@font-face { font-family: \"DroidSans\", serif, sans-serif; src:url(res:///ebook/fonts/DroidSansFallback.ttf); }");
                        sw.WriteLine("</style>");
                        sw.WriteLine("</head>");
                        sw.WriteLine("<body>");
                        ended = false;
                    }
                }
                using (StreamWriter sw = new StreamWriter(output_file, true, Encoding.GetEncoding("UTF-16"))) {
                    line = line.Replace("&", "&amp;");
                    line = line.Replace(" ", "&nbsp;");
                    sw.WriteLine(line + "<br/>");
                }
                lineCount++;
                if (lineCount % separationLineCount == separationLineCount - 1) {
                    using (StreamWriter sw = new StreamWriter(output_file, true, Encoding.GetEncoding("UTF-16"))) {
                        sw.WriteLine("</body>");
                        sw.WriteLine("</html>");
                    }
                    fileCount++;
                    ended = true;
                }
            }

            if (!ended && output_file != null) {
                using (StreamWriter sw = new StreamWriter(output_file, true, Encoding.GetEncoding("UTF-16"))) {
                    sw.WriteLine("</body>");
                    sw.WriteLine("</html>");
                }
                fileCount++;
            }
        }

        String bookId = Guid.NewGuid().ToString();

        string content_file = oebps_directory + Path.DirectorySeparatorChar + "content.opf";
        using (StreamWriter sw = new StreamWriter(content_file, false, Encoding.GetEncoding("UTF-8"))) {
            sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>");
            sw.WriteLine("<package xmlns=\"http://www.idpf.org/2007/opf\" unique-identifier=\"BookId\" version=\"2.0\">");
            sw.WriteLine("<metadata xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:opf=\"http://www.idpf.org/2007/opf\">");
            sw.WriteLine("<dc:identifier id=\"BookId\" opf:scheme=\"UUID\">urn:uuid:"+bookId+"</dc:identifier>");
            sw.WriteLine("<dc:title>"+title+"</dc:title>");
            sw.WriteLine("<meta content=\"0.4.2\" name=\"Sigil version\" />");
            sw.WriteLine("</metadata>");
            sw.WriteLine("<manifest>");
            sw.WriteLine("<item id=\"ncx\" href=\"toc.ncx\" media-type=\"application/x-dtbncx+xml\" />");
            for (int i = 0; i < fileCount; i++) {
                sw.WriteLine("<item href=\""+i+".xhtml\" id=\"page"+i+"\" media-type=\"application/xhtml+xml\" />");
            }
            sw.WriteLine("</manifest>");
            sw.WriteLine("<spine toc=\"ncx\">");
            for (int i = 0; i < fileCount; i++) {
                sw.WriteLine("<itemref idref=\"page"+i+"\" />");
            }
            sw.WriteLine("</spine>");
            sw.WriteLine("</package>");
        }

        string toc_file = oebps_directory + Path.DirectorySeparatorChar + "toc.ncx";
        using (StreamWriter sw = new StreamWriter(toc_file, false, Encoding.GetEncoding("UTF-8"))) {
            sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\" ?><!DOCTYPE ncx PUBLIC \"-//NISO//DTD ncx 2005-1//EN\" \"http://www.daisy.org/z3986/2005/ncx-2005-1.dtd\"><ncx xmlns=\"http://www.daisy.org/z3986/2005/ncx/\" version=\"2005-1\">");
            sw.WriteLine("<head>");
            sw.WriteLine("<meta content=\"urn:uuid:"+bookId+"\" name=\"dtb:uid\"/>");
            sw.WriteLine("<meta content=\"1\" name=\"dtb:depth\"/>");
            sw.WriteLine("<meta content=\"0\" name=\"dtb:totalPageCount\"/>");
            sw.WriteLine("<meta content=\"0\" name=\"dtb:maxPageNumber\"/>");
            sw.WriteLine("</head>");
            sw.WriteLine("<docTitle>");
            sw.WriteLine("<text>"+title+"</text>");
            sw.WriteLine("</docTitle>");
            sw.WriteLine("<navMap>");
            for (int i = 0; i < fileCount; i++) {
                sw.WriteLine("<navPoint id=\"navPoint-"+i+"\" playOrder=\""+(i+1)+"\">");
                sw.WriteLine("<navLabel>");
                sw.WriteLine("<text>Part "+i+"</text>");
                sw.WriteLine("</navLabel>");
                sw.WriteLine("<content src=\""+i+".xhtml\"/>");
                sw.WriteLine("</navPoint>");
            }
            sw.WriteLine("</navMap>");
            sw.WriteLine("</ncx>");
        }
    }
}
