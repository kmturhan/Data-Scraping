/*
    Öncelikle iki tür sorunum var.
 1-     Verileri HTML kod olarak çektiğimde bana Türkçe karakterlerde sorun çıkartıyor.
     Encoding utf-8, iso-8859-9 falan da denedim sorunu düzeltemedim. Böyle olunca veritabanına da
     bu şekilde kaydoluyor. Orada bir hatam var.
    
 2-     Sitelerde link ayıklama yapmak istediğimde bana sürekli page=1  sayfasının linklerini döndürüyor.
     özellikle page=2 sayfasının html kodunu çekmek istediğimde bile page=1 geliyor. Fakat burda yanlış
     düşünüyor da olabilirim. Bir ipucu yakaladım diye düşünüyorum. Ben page=2 sayfasına gittiğimde 
     (çok hızlı gerçekleşiyor bu) ilk önce page=1 listesi önüme gelip daha sonra page=2 'ye gidiyor.
     Burada bir gecikme yaparsam belki page=2 'ye ulaşabilirim diye düşündüm. Async await gibi olaylar
     vardı. Denedim fakat başarılı olamadım.

        *Kayıtlar klasöründe 3 adet resim koydum. Oradan da inceleyebilirsiniz.

     Url girişini bölümlere göre ele aldım. Örneğin;
     http://www.dr.com.tr/CokSatanlar/Kitap
     http://www.dr.com.tr/Kategori_/Kitap/En-Yeniler/10001/3
     http://www.dr.com.tr/kategori/Kitap/Edebiyat/grupno=00055


        İlgilendiğiniz için teşekkür ederim.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Net;
using System.IO;
using System.Data.OleDb;
using System.Data;
namespace ConsoleApp5
{
    class Program
    {
        static void Main(string[] args)
        {
            //OleDbConnection baglanti = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=Kitaplik.mdb;");
            //baglanti.Open();
            //OleDbDataAdapter adap = new OleDbDataAdapter("select * from urun",baglanti);
            //DataSet ds = new DataSet();
            //adap.Fill(ds);



            aaEntities ctx = new aaEntities();
            List<data> liste = ctx.data.ToList();
            var wc = new WebClient();
            HtmlDocument doc = new HtmlDocument();
            List<string> linkler = new List<string>();
            int kitapSayisi = 0;

            htmlKaynakKod();
            sayfaSayisi();
            urlOgren();
            Kaydet();

            Console.ReadLine();
            //Console.WriteLine(ktp[0].urunAdi);

            //foreach (var item in ds.Tables)
            //{
            //    Console.WriteLine(item);
            //    Console.WriteLine("----------------------------------------");
            //}

            //string url = "http://www.dr.com.tr/kategori/wattpad-kitaplari#/page=" + i.ToString();
            //string htmlContent = wc.DownloadString(url);
            //doc.LoadHtml(htmlContent);

            //File.WriteAllLines(@"C:\Users\kmtur\Desktop\bb.txt", liste);


            void htmlKaynakKod()
            {
                Console.WriteLine("URL Giriniz :");
                string url = Console.ReadLine();

                wc.Encoding = System.Text.Encoding.UTF8;
                string site = wc.DownloadString(url);

                //File.WriteAllText(@"C:\Users\kmtur\Desktop\aa.txt", site);
                //File.WriteAllText(@"C:\Users\kmtur\Desktop\bb.txt", site);

                doc.LoadHtml(site);

                Console.WriteLine(site);
            }

            void sayfaSayisi()
            {
                var node = doc.DocumentNode.SelectSingleNode("//*[@id='searchResultCount']");
                int sayfaSay = 0;
                kitapSayisi = int.Parse(node.InnerText.Replace("ADET", ""));

                if (kitapSayisi % 42 == 0)
                    sayfaSay = kitapSayisi / 42;
                else
                    sayfaSay = (kitapSayisi / 42) + 1;
            }

            void urlOgren()
            {
                int sayac = 0;
                foreach (var link in doc.DocumentNode.SelectNodes("//a[@class='item-name']"))
                {

                    string href = link.GetAttributeValue("href", string.Empty);
                    Console.WriteLine(++sayac + ") " + href);
                    linkler.Add(href);
                }
            }
            void Kaydet()
            {
                data dt = new data();
                string site = null;
                int sayac = 0;
                foreach (var aa in linkler)
                {
                    wc.Encoding = System.Text.Encoding.GetEncoding(1254);
                    // wc.Encoding = System.Text.Encoding.UTF8;
                    site = wc.DownloadString("http://www.dr.com.tr" + aa);
                    wc.Encoding = System.Text.Encoding.GetEncoding(1254);
                    doc.LoadHtml(site);


                    string urunNoo = aa.Substring(aa.Length - 14, 13).ToString();
                    string urunAdi = doc.DocumentNode.SelectSingleNode("//*[@id='catPageContent']/section[2]/div[1]/div/div[1]/div/h3").InnerText;
                    string Yazar = doc.DocumentNode.SelectSingleNode("//*[@id='catPageContent']/section[2]/div[2]/div[2]/div[2]/div[1]/span/a/span").InnerText;
                    string Yayinevi = doc.DocumentNode.SelectSingleNode("//*[@id='publisherName']").InnerText;
                    string Fiyati = doc.DocumentNode.SelectSingleNode("//*[@id='sta0']/div/p/text()").InnerText;
                    string ozet = doc.DocumentNode.SelectSingleNode("//*[@id='catPageContent']/section[2]/div[2]/div[3]").InnerText;
                    string Linkler = "http://www.dr.com.tr" + aa;

                    StreamWriter writetxt;
                    writetxt = File.AppendText(@"Page.txt");
                    writetxt.WriteLine("Ürün No : " + urunNoo + Environment.NewLine + "Ürün Adı : " + urunAdi +
                        Environment.NewLine + "Yazar : " + Yazar + Environment.NewLine + "Yayınevi : " + Yayinevi +
                        Environment.NewLine + "Fiyatı : " + Fiyati +
                        Environment.NewLine + "URL : " + Linkler + Environment.NewLine +
                        Environment.NewLine + "Özet : " + Environment.NewLine + ozet + Environment.NewLine + Environment.NewLine);
                    writetxt.WriteLine("----------------------------------------------------------------------");
                    writetxt.Close();
                    Console.WriteLine(++sayac + ". Kitap not defterine işlendi.");
                    //OleDbCommand cmd = new OleDbCommand("INSERT INTO urun VALUES ("+urunNo+","+urunAdi+","+Yazar+","+Yayinevi+","+Fiyati+","+Ozet+","+Linkler+")",baglanti);
                    //cmd.ExecuteNonQuery();
                    //baglanti.Close();
                    dt.uNo = urunNoo; dt.uAdi = urunAdi; dt.Yazar = Yazar; dt.Yayinevi = Yayinevi; dt.Fiyat = Fiyati; dt.Url = Linkler; dt.Ozet = ozet;

                    ctx.data.Add(dt);
                    ctx.SaveChanges();

                }
                Console.WriteLine("Bu kategoride {0} adet kitap bulunmaktadır.", kitapSayisi);
                
                //Console.WriteLine(kitapSayisi + " " + "kitap içinden " + sayac + " adet veritabanına eklendi.");

            }
        }
    }
}
