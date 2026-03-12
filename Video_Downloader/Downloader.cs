using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Video_Downloader.Properties;

namespace Video_Downloader
{
    public partial class Downloader : Form
    {
        public Downloader()
        {
            InitializeComponent();
        }

        private void Downloader_Load(object sender, EventArgs e)
        {
            this.Text = "Zylodux Video Downloader";
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            LogAna("Hoş geldin Zylodux, motor çalışıyor.");

            if (string.IsNullOrEmpty(Settings.Default.YtDlpYolu))
            {
                LogAyar("DİKKAT: yt-dlp yolu seçilmemiş!", "HATA");
            }
        }

        private void LogYaz(RichTextBox hedefKutu, string mesaj, string baslik)
        {
            if (hedefKutu.InvokeRequired)
            {
                hedefKutu.Invoke(new Action(() => LogYaz(hedefKutu, mesaj, baslik)));
                return;
            }

            string m = mesaj.ToLower();
            string saat = DateTime.Now.ToString("HH:mm:ss");
            string cikti = mesaj.Trim();

            // Filtreler
            if (m.Contains("already been downloaded")) cikti = "BU PARÇA ZATEN KÜTÜPHANENDE VAR!";
            else if (m.Contains("destination:")) cikti = "Dosya oluşturuluyor...";

            // Log ekleme
            if (baslik == "Yüzde")
            {
                if (mesaj.Contains("20.0%") || mesaj.Contains("40.0%") || mesaj.Contains("60.0%") || mesaj.Contains("80.0%") || mesaj.Contains("100%"))
                {
                    hedefKutu.AppendText($"[{saat}] İşlem: İlerleme {mesaj.Trim()}{Environment.NewLine}");
                }
            }
            else
            {
                hedefKutu.AppendText($"[{saat}] {baslik}: {cikti}{Environment.NewLine}");
            }

            // --- OTOMATİK KAYDIRMA (Standart RTB'de tıkır tıkır çalışır) ---
            hedefKutu.SelectionStart = hedefKutu.Text.Length;
            hedefKutu.ScrollToCaret();
        }
        private void LogAna(string mesaj, string baslik = "Tezgâh") => LogYaz(rchlogdowlnloader, mesaj, baslik);
        private void LogAyar(string mesaj, string baslik = "Ayarlar") => LogYaz(rchlogsettings, mesaj, baslik);

        private void nightForm1_Click(object sender, EventArgs e)
        {

        }

        private void btntoolslocation_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Executable Files (*.exe)|*.exe";
                ofd.Title = "yt-dlp.exe dosyasını bulun";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    Settings.Default.YtDlpYolu = ofd.FileName;
                    Settings.Default.Save();
                    LogAyar("yt-dlp yolu başarıyla mühürlendi.", "Ayarlar");
                }
            }
        }

        private void btnsaveitemlocation_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Müziklerin indirileceği klasörü seçin";

                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    Settings.Default.KayitYolu = fbd.SelectedPath;
                    Settings.Default.Save();
                    LogAyar($"Hedef klasör seçildi: {fbd.SelectedPath}", "Ayarlar");
                }
            }
        }

        private void btnsavesettings_Click(object sender, EventArgs e)
        {
            Settings.Default.Save();
            LogAyar("Değişiklikler başarıyla kaydedildi.");
        }

        private async void btndownloader_Click(object sender, EventArgs e)
        {
            string url = lbllink.Text;
            if (string.IsNullOrEmpty(url))
            {
                LogAna("Lütfen bir YouTube linki yapıştırın!", "Uyarı");
                return;
            }

            string arguments = "";
            string formatEtiketi = "";

         

            if (rbtnmp3.Checked)
            {
              
                arguments = "-x --audio-format mp3 --concurrent-fragments 16";
                formatEtiketi = "MP3";
            }
            else if (rbtnmp3h.Checked)
            {
              
                arguments = "-x --audio-format mp3 --audio-quality 0 --concurrent-fragments 16";
                formatEtiketi = "MP3 HD";
            }
            else if (rbtnmp4.Checked)
            {
               
                arguments = "-f \"bestvideo[ext=mp4]+bestaudio[ext=m4a]/best[ext=mp4]/best\" --concurrent-fragments 16";
                formatEtiketi = "MP4";
            }
            else if (rbtnmp4h.Checked)
            {
              
                arguments = "-f \"bestvideo[ext=mp4]+bestaudio[ext=m4a]/best[ext=mp4]/best\" --merge-output-format mp4 --concurrent-fragments 16";
                formatEtiketi = "MP4 HD";
            }

            if (string.IsNullOrEmpty(arguments))
            {
                LogAna("Lütfen bir format seçin!", "Uyarı");
                return;
            }

       

            await BaslatIndirme(url, arguments, formatEtiketi);

         
        }


        private Process activeProcess = null;
        private async Task BaslatIndirme(string url, string args, string etiket)
        {
            string ytDlpYolu = global::Video_Downloader.Properties.Settings.Default.YtDlpYolu;
            string kayitYolu = global::Video_Downloader.Properties.Settings.Default.KayitYolu;

            LogAna($"{etiket} işlemi başlatıldı...", "Tezgâh");

            await Task.Run(() =>
            {
                try
                {
                    activeProcess = new Process();

                   
                    activeProcess.StartInfo.FileName = ytDlpYolu;
                    activeProcess.StartInfo.Arguments = $"--no-warnings {args} -o \"{kayitYolu}/%(title)s.%(ext)s\" {url}";
                    activeProcess.StartInfo.UseShellExecute = false;
                    activeProcess.StartInfo.CreateNoWindow = true;
                    activeProcess.StartInfo.RedirectStandardOutput = true;
                    activeProcess.StartInfo.RedirectStandardError = true;

                  
                    activeProcess.OutputDataReceived += (s, ev) =>
                    {
                        if (ev.Data != null)
                        {
                            string d = ev.Data;
                            string dl = d.ToLower();

                           
                            if (dl.Contains("[download] destination:"))
                            {
                                if (dl.Contains(".f") || dl.Contains(".mp4"))
                                    LogAna("Görüntü dosyası çekiliyor...", "İşlem");
                                else if (dl.Contains(".m4a") || dl.Contains(".webm") || dl.Contains(".mp3"))
                                    LogAna("Ses dosyası çekiliyor...", "İşlem");
                            }
                           
                            else if (d.Contains("%"))
                            {
                                if (d.Contains("25.") || d.Contains("50.") || d.Contains("75."))
                                    LogAna("İlerleme: %" + ExtractPercent(d), "Motor");
                                else if (d.Contains("100%"))
                                    LogAna("İndirme %100 tamamlandı. Motor sıcak, dönüştürme başlıyor...", "İşlem");
                            }
                          
                            else if (dl.Contains("[extractaudio]") || dl.Contains("converting"))
                                LogAna("Sinyaller işleniyor, dosya mühürleniyor... 🎶", "İşlem");
                            else if (dl.Contains("merging"))
                                LogAna("Görüntü ve ses birleştiriliyor... 🎬", "İşlem");
                            else if (dl.Contains("deleting original"))
                                LogAna("Geçici dosyalar temizleniyor, paket hazır!", "Sistem");
                        }
                    };

                 
                    activeProcess.Start();

                    if (!activeProcess.HasExited)
                    {
                        activeProcess.PriorityClass = ProcessPriorityClass.High;
                    }

                    activeProcess.BeginOutputReadLine();
                    activeProcess.BeginErrorReadLine();

               
                    activeProcess.WaitForExit();

            
                    if (activeProcess != null)
                    {
                        LogAna("Tüm liste/video işlemleri başarıyla bitti!", "Tezgâh");
                    }
                }
                catch (Exception ex)
                {
                    LogAna("Motor hatası: " + ex.Message, "Hata");
                }
                finally
                {
                    activeProcess = null;
                }
            });
        }

        private string ExtractPercent(string text)
        {
            try
            {
                int start = text.IndexOf("[download]") + 10;
                int end = text.IndexOf("%") + 1;
                return text.Substring(start, end - start).Trim();
            }
            catch { return ""; }
        }
        private void btnstop_Click(object sender, EventArgs e)
        {
            if (activeProcess != null && !activeProcess.HasExited)
            {
                try
                {

                    activeProcess.Kill(true);
                    LogAna("OPERASYON İPTAL EDİLDİ! Motor durduruldu.", "Sistem");
                    System.Media.SystemSounds.Hand.Play();
                }
                catch (Exception ex)
                {
                    LogAna("Durdurma sırasında hata: " + ex.Message, "Hata");
                }
            }
            else
            {
                LogAna("Zaten çalışan bir motor yok, neyi durduruyorsun aga?", "Uyarı");
            }
        }

        private void nightLinkLabel1_DoubleClick(object sender, EventArgs e)
        {
            try
            {

                Process.Start(new ProcessStartInfo("https://github.com/Zylodux") { UseShellExecute = true });

              
                lnklblgithub.LinkVisited = true;
            }
            catch (Exception ex)
            {
                LogAna("Tarayıcı başlatılamadı: " + ex.Message, "Hata");
            }
        }

        private void lnklblgithub_Click(object sender, EventArgs e)
        {
            try
            {

                Process.Start(new ProcessStartInfo("https://github.com/Zylodux") { UseShellExecute = true });

                lnklblgithub.LinkVisited = true;
            }
            catch (Exception ex)
            {
                LogAna("Tarayıcı başlatılamadı: " + ex.Message, "Hata");
            }
        }
    }
}
