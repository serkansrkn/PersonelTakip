using DocumentFormat.OpenXml.Spreadsheet;
using StructureMap.Building;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PersonelTakip.Model;

namespace PersonelTakip
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        //SqlConnection con;
        //SqlCommand cmd;
        //string connectionString = @"Data Source = .\SQLEXPRESS01 ; Initial Catalog=PersonelDB; Integrated Security = SSPI;";

        int personelid = 0;

        private void Form1_Load(object sender, EventArgs e)
        {
            birimLoad();
            personelLoad();
        }

        void birimLoad()
        {
            /*
            try
            {
                con = new SqlConnection(connectionString);
                cmd = new SqlCommand("select * from birimler", con);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow row in dt.Rows)
                {
                    cmbxBirim.Items.Add(row["birimler"].ToString());
                }
            }
            catch (SqlException ex)
            {
                throw ex;
            }*/

            foreach (DataRow row in IDataBase.DataToDataTable("select * from birimler").Rows)
            {
                cmbxBirim.Items.Add(row["birimler"].ToString());
            }
        }

        void personelLoad() //penceredeki tabloyu veri tabanı ile senkronize etmemiz lazım. yani veri tananında ne var ne yok diye bir arayüz.
        {
            /*
            try
            {
                con = new SqlConnection(connectionString);
                cmd = new SqlCommand("select * from personeller", con);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dtgrid.DataSource = dt;                  //verileri çekip tabloya yazdırdık

                dtgrid.Columns["id"].Visible = false;

            }
            catch (SqlException ex)
            {
                throw ex;
            }*/

            dtgrid.DataSource = IDataBase.DataToDataTable("select * from personeller");
            dtgrid.Columns["id"].Visible = false;
        }

        void personelEkle()
        {
            /*
            try
            {
                con = new SqlConnection(connectionString);
                cmd = new SqlCommand("insert into personeller (sicil, ad, soyad, cinsiyet, dogumTarihi, birim, telefon,adres) values (@sicil, @ad, @soyad, @cinsiyet, @dogumTarihi, @birim, @telefon, @adres)",con);
                con.Open();

                ekle();

                cmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                throw ex;
            }
            finally
            {
                con.Close();
            }*/

            List<SqlParameter> parameters = new List<SqlParameter>();

            int checkSicil;
            var valueSicil = (object)DBNull.Value;
            if (int.TryParse(maskedTextBox1.Text, System.Globalization.NumberStyles.None, null, out checkSicil))
            {
                valueSicil = checkSicil;
            }
            parameters.Add(new SqlParameter("@sicil",valueSicil));

            parameters.Add(new SqlParameter("@ad", txtAdi.Text));     //SqlDbType.VarChar) { Value = txtAdi.Text });
            parameters.Add(new SqlParameter("@soyad", SqlDbType.VarChar) { Value = txtSoyadi.Text });

            string cinsiyet = "";

            if (radiobtnErkek.Checked)
            {
                cinsiyet = radiobtnErkek.Text;
            }
            else if (radiobtnKadin.Checked)
            {
                cinsiyet = radiobtnKadin.Text;
            }
            parameters.Add(new SqlParameter("@cinsiyet", SqlDbType.VarChar) { Value = cinsiyet });

            DateTime checkDate;
            var valueDate = (object)DBNull.Value;

            if (DateTime.TryParseExact(mtxtDgmTrihi.Text, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out checkDate))
            {
                valueDate = checkDate;
            }
            parameters.Add(new SqlParameter("@dogumTarihi",valueDate));

            parameters.Add(new SqlParameter("@birim", SqlDbType.VarChar) { Value = cmbxBirim.Text });
            parameters.Add(new SqlParameter("@telefon", SqlDbType.VarChar) { Value = mtxtTelefon.Text });
            parameters.Add(new SqlParameter("@adres", SqlDbType.VarChar) { Value = txtAdres.Text });

            IDataBase.executeNonQuery("insert into personeller (sicil, ad, soyad, cinsiyet, dogumTarihi, birim, telefon, adres) values (@sicil, @ad, @soyad, @cinsiyet, @dogumTarihi, @birim, @telefon, @adres)",  parameters);

            personelLoad();
        }

        private void BtnEkle_Click(object sender, EventArgs e)
        {
            if (personelid == 0)
            {
                personelEkle();
                temizle();
            }
            else
            {
                MessageBox.Show("Seçili Personel Var");
            }
        }

        private void Dtgrid_CellClick(object sender, DataGridViewCellEventArgs e) //tabloda tıkladığımız satır bilgileri kutucuklara yazılsın
        {
            if (e.RowIndex > -1) //eğer seçilen bir satır varsa demek
            {
                personelid = Convert.ToInt32(dtgrid.Rows[e.RowIndex].Cells["id"].Value); // id kolonundan seçilen satırın satır no'sunu al

                radiobtnKadin.Checked = false;
                radiobtnErkek.Checked = false;

                foreach (DataRow row in IDataBase.DataToDataTable("select * from personeller where id = @id",
                    new SqlParameter("@id", SqlDbType.Int) { Value = personelid }).Rows)  //bu satırdaki bilgileri tek tek kutulara set et
                {
                    maskedTextBox1.Text = row["sicil"].ToString();
                    txtAdi.Text = row["ad"].ToString();
                    txtSoyadi.Text = row["soyad"].ToString();
                    string cinsiyet = row["cinsiyet"].ToString();

                    if (cinsiyet == "Erkek")
                    {
                        radiobtnErkek.Checked = true;
                    }
                    else if (cinsiyet == "Kadın")
                    {
                        radiobtnKadin.Checked = true;
                    }

                    mtxtDgmTrihi.Text = string.Format(("{0:dd/MM/yyyy}"), row["dogumTarihi"]); //sadece row["dogumTarihi"]).ToString(); yazarsak tarih kayabilir
                    cmbxBirim.Text = row["birim"].ToString();
                    mtxtTelefon.Text = row["telefon"].ToString();
                    txtAdres.Text = row["adres"].ToString();

                    /*
                    try
                    {
                        con = new SqlConnection(connectionString);
                        cmd = new SqlCommand("select * from personeller where id = @id", con);
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = personelid;  //tabloda seçilen id satırını, veri tabanında bul
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);                   //bulduk aldık satırı

                        radiobtnKadin.Checked = false;
                        radiobtnErkek.Checked = false;

                        foreach (DataRow row in dt.Rows)  //bu satırdaki bilgileri tek tek kutulara set et
                        {
                            maskedTextBox1.Text = row["sicil"].ToString();
                            txtAdi.Text = row["ad"].ToString();
                            txtSoyadi.Text = row["soyad"].ToString();
                            string cinsiyet = row["cinsiyet"].ToString();

                            if(cinsiyet == "Erkek")
                            {
                                radiobtnErkek.Checked = true;
                            }
                            else if(cinsiyet=="Kadın")
                            {
                                radiobtnKadin.Checked = true;
                            }
                            mtxtDgmTrihi.Text = string.Format(("{0:dd/MM/yyyy}"), row["dogumTarihi"]); //sadece row["dogumTarihi"]).ToString(); yazarsak tarih kayabilir
                            cmbxBirim.Text = row["birim"].ToString();
                            mtxtTelefon.Text = row["telefon"].ToString();
                            txtAdres.Text = row["adres"].ToString();
                        }
                    }
                    catch (SqlException ex)
                    {
                        throw ex;
                    }*/
                }
            }
        }


        void personelGuncelle()
        {
            /*
            try
            {
                con = new SqlConnection(connectionString);
                cmd = new SqlCommand("update personeller set sicil = @sicil, ad = @ad, soyad = @soyad, cinsiyet = @cinsiyet, dogumTarihi = @dogumTarihi, birim = @birim, telefon = @telefon, adres = @adres where id = @id", con);

                con.Open();

                ekle();

                cmd.Parameters.Add("@id", SqlDbType.Int).Value = personelid;
                cmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                throw ex;
            }
            finally
            {
                con.Close();
            }*/

            List<SqlParameter> parameters = new List<SqlParameter>();

            int checkSicil;
            var valueSicil = (object)DBNull.Value;
            if (int.TryParse(maskedTextBox1.Text, System.Globalization.NumberStyles.None, null, out checkSicil))
            {
                valueSicil = checkSicil;
            }
            parameters.Add(new SqlParameter("@sicil", valueSicil));

            parameters.Add(new SqlParameter("@ad", SqlDbType.VarChar) { Value = txtAdi.Text });
            parameters.Add(new SqlParameter("@soyad", SqlDbType.VarChar) { Value = txtSoyadi.Text });

            string cinsiyet = "";

            if (radiobtnErkek.Checked)
            {
                cinsiyet = radiobtnErkek.Text;
            }
            else if (radiobtnKadin.Checked)
            {
                cinsiyet = radiobtnKadin.Text;
            }
            parameters.Add(new SqlParameter("@cinsiyet", SqlDbType.VarChar) { Value = cinsiyet });

            DateTime checkDate;
            var valueDate = (object)DBNull.Value;

            if (DateTime.TryParseExact(mtxtDgmTrihi.Text, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out checkDate))
            {
                valueDate = checkDate;
            }
            parameters.Add(new SqlParameter("@dogumTarihi", valueDate));

            parameters.Add(new SqlParameter("@birim", SqlDbType.VarChar) { Value = cmbxBirim.Text });
            parameters.Add(new SqlParameter("@telefon", SqlDbType.VarChar) { Value = mtxtTelefon.Text });
            parameters.Add(new SqlParameter("@adres", SqlDbType.VarChar) { Value = txtAdres.Text });
            parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Value = personelid });

            IDataBase.executeNonQuery("update personeller set sicil = @sicil, ad = @ad, soyad = @soyad, cinsiyet = @cinsiyet, dogumTarihi = @dogumTarihi, birim = @birim, telefon = @telefon, adres = @adres where id = @id", parameters);

            personelLoad();
        }

        private void BtnGuncelle_Click(object sender, EventArgs e)
        {
            if (personelid > 0)
            {
                personelGuncelle();
            }
            else
            {
                MessageBox.Show("Personel Seçiniz");
            }
        }

        void personelSil()
        {
            /*
            try
            {
                con = new SqlConnection(connectionString);
                cmd = new SqlCommand("delete personeller where id = @id", con);

                con.Open();

                cmd.Parameters.Add("@id", SqlDbType.Int).Value = personelid;
                cmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                throw ex;
            }

            finally
            {
                con.Close();
            }*/

            IDataBase.executeNonQuery("delete personeller where id = @id", new SqlParameter("@id", SqlDbType.Int) { Value = personelid });

            personelLoad();
        }

        private void BtnSil_Click(object sender, EventArgs e)
        {
            if (personelid > 0)
            {
                DialogResult dialog = MessageBox.Show("Personeli Silmek İstediğinize Emin Misiniz?", "Personel Sil", MessageBoxButtons.YesNo);

                if (dialog == DialogResult.Yes)
                {
                    personelSil();
                    temizle();

                }
                else
                {
                    MessageBox.Show("Silme İşlemi İptal Edildi");
                }
            }
            else
            {
                MessageBox.Show("Personel Seçiniz");
            }
        }

        void temizle()
        {
            personelid = 0;

            foreach (var item in tableLayoutPanel1.Controls)
            {
                if (item is TextBox)
                {
                    ((TextBox)item).Text = "";
                }

                if (item is MaskedTextBox)
                {
                    ((MaskedTextBox)item).Text = "";
                }

                if (item is ComboBox)
                {
                    ((ComboBox)item).Text = "";
                }
            }

            foreach (var item in tableLayoutPanel2.Controls)
            {
                if (item is RadioButton)
                {
                    ((RadioButton)item).Checked = false;
                }
            }
        }

        private void BtnTemizle_Click(object sender, EventArgs e)
        {
            temizle();
        }
}   }