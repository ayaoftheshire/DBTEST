﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using System.IO;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Drawing.Text;
using System.Windows.Markup;


namespace Drevo_Project
{
    public partial class Main : Form
    {
        public String ContNum { get; set; }
        public String ContMail { get; set; }

        ConnectBD sql = new ConnectBD();
        public Main()
        {
            InitializeComponent();
            DrawTreeBmp();


        }

        private void Main_Load(object sender, EventArgs e)
        {
            try
            {
                sql.command.CommandText = "SELECT * FROM User WHERE id='" + DataClass.ID + "' ";
                //SQLiteDataAdapter adapter1 = new SQLiteDataAdapter(sql.command.Connection);

                SQLiteDataReader read = sql.command.ExecuteReader();

                while (read.Read())
                {
                    DataClass.CardID = read["idCard"].ToString();
                }
                read.Close();

                sql.command.CommandText = "SELECT * FROM Card WHERE id = '" + DataClass.CardID + "' ";
                SQLiteDataReader read2 = sql.command.ExecuteReader();
                while (read2.Read())
                {
                    labelFio.Text = read2["surname"].ToString() + " " + read2["name"].ToString() + " " + read2["middlename"].ToString();

                    listBoxBio.Items.Add(Convert.ToString(read2["bio"]));
                    if (read2["deathday"].ToString() == "")
                    {
                        labelDataBorn.Text = read2["birthday"].ToString() + "-...";
                    }
                    else
                    {
                        labelDataBorn.Text = read2["birthday"].ToString() + "-" + read2["deathday"].ToString();
                    }
                }
                read2.Close();

                sql.command.CommandText = "SELECT * FROM Card WHERE id = '" + DataClass.CardID + "' ";
                SQLiteDataReader read3 = sql.command.ExecuteReader();

                while (read3.Read())
                {
                    NumberBox.Text = read3["number"].ToString();
                    ContNum = read3["number"].ToString();
                    MailBox.Text = read3["mail"].ToString();
                    ContMail = read3["mail"].ToString();

                }
                read3.Close();


            }

            catch (SQLiteException ex)
            {
                MessageBox.Show("Error: стр рег" + ex.Message);
            }
        }

        private void ChangeContactsButton_Click(object sender, EventArgs e)
        {
            if (NumberBox.Text != ContNum || MailBox.Text != ContMail)
            {
                try
                {
                    sql.command.CommandText = "UPDATE Card SET number= '" + NumberBox.Text + "', mail='" + MailBox.Text + "' WHERE id = '" + DataClass.CardID + "'  ";
                    sql.command.ExecuteNonQuery();
                }
                catch (SQLiteException ex)
                {
                    MessageBox.Show("Error: стр рег" + ex.Message);
                }
            }
        }


        private void buttonEdit_Click(object sender, EventArgs e) //реализовать после вывода данных в БИО табличку админа
        {
            EditCard editCard = new EditCard();
           

            if (editCard.ShowDialog() == DialogResult.OK)
            {
                editCard.Close();
            }
        }

        private void buttonAddCard_Click(object sender, EventArgs e) //кнопка добавления человека в дерево
        {
            NewCard newCard = new NewCard();

            if (newCard.ShowDialog() == DialogResult.OK)
            {
                newCard.Close();
                //вызвать новую отрисовку древа
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
            Application.Exit();
        }

        public int[][] GetData() //получение данных в таблицу
        {
            DataTable dt = new DataTable();
            String sqlQuery;

            try
            {
                sqlQuery = "SELECT Generation, id, idPartner, idMom, idDad FROM Card ORDER BY Generation";
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(sqlQuery, sql.connect);
                adapter.Fill(dt);

            }
            catch (SQLiteException ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }


            int rows = dt.Rows.Count;
            int columns = dt.Columns.Count;


            int[][] arr = new int[rows][];


            for (int i = 0; i < rows; i++)
            {
                DataRow row = dt.Rows[i];
                arr[i] = new int[columns];
                for (int j = 0; j < columns; j++)
                {

                    arr[i][j] = Convert.ToInt32(row[j]);
                }
            }

            return arr;

        }

        /*private List<String> GetNames() создать класс и изменить всё
        {
            sql.command.CommandText = "SELECT id,surname|| ' ' || name|| ' ' || middlename, gender FROM Card WHERE id >= 1";
            List<String> Names = new List<String>();
            try
            {
                SQLiteDataReader r = sql.command.ExecuteReader();

                while (r.Read())
                {
                    Person entity = new Person
                    {
                        Id = r.GetInt32(0),
                        Name = r.GetString(1),
                        Gender = r.GetInt32(2)
                    };
                    Names.Add(entity);
                }
                r.Close();
                sql.command.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }

            return Names;
        }*/

        private List<List<int[]>> SortDataToGeneration() //преобразование в список по поколениям
        {
            int[][] values = GetData();
            int rowsVal = values.Length;
            int columnsVal = values[0].Length;

            List<List<int[]>> generation = new List<List<int[]>> { //количество поколений 3 например. сделать динамические изменения поколений
                new List<int[]>(),
                new List<int[]>(),
                new List<int[]>()
            };

            for (int i = 1; i < rowsVal; i++)
            {
                if (values[i][0] == 0)
                {
                    generation[0].Add(values[i]);
                }
                else if (values[i][0] == 1)
                {
                    generation[1].Add(values[i]);
                }
                else if (values[i][0] == 2)
                {
                    generation[2].Add(values[i]);
                }
                else if (values[i][0] == 3)
                {
                    generation[3].Add(values[i]);
                }

            }

            return generation;
        }

        private void DrawTreeBmp()
        {
            List<List<int[]>> gener = SortDataToGeneration(); //[generation, id, idPartner, idMom, idDad]

            int length = gener.Count; //количество поколений 3
            int[] temp = new int[length];

            for (int i = 0; i < length; i++)
            {
                temp[i] = gener[i].Count;
            }

            int maxCounGen = temp.Max(); //максимальная длина поколения

            panelTree.AutoScroll = true; //автоскролл
            pictureBoxTree.SizeMode = PictureBoxSizeMode.AutoSize;

            Bitmap bmp = new Bitmap(maxCounGen * 320, length * 220);//размер битмапа под количество элементов
            Graphics graph = Graphics.FromImage(bmp);
            Pen pen = new Pen(Color.Gray, 1f);
            Pen pen1 = new Pen(Color.Black, 2f);


            int x = 10, y = 10; //положение точки для верхнего поколения
            int stepHor = 250; //блок для одного чела с партнером
            int stepVert = 200; //блок для пары в высоту
            pictureBoxTree.Image = bmp;

            Font fnt = new System.Drawing.Font("Arial", (float)10);
            Brush br = new SolidBrush(Color.Red);
            
            
            

            for (int i = length - 1; i >= 0; i--) //прорисовка блоков цикл по поколениям
            {
                for (int k = 0; k < gener[i].Count; k++) //цикл по массивам внутри поколений
                {

                    //graph.DrawRectangle(pen, x, y, 300, 200);
                    string m = k.ToString();
                    graph.DrawEllipse(pen1, x, y, 100, 100);
                    graph.DrawString(m, fnt, br, x + 20, y - 10);

                    /*if (gener[i][0][1] == gener[i][1][2])
                    {
                        graph.DrawLine(pen1, x + 100, y + 50, x + stepHor, y + 50);
                    }*/
                    x += stepHor;

                }
                x = 10;
                y += stepVert;
            }



        }

        
    }

}
