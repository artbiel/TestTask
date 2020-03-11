using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Test.Models;

namespace Test
{
    public partial class Form1 : Form
    {
        //Кэш для списка студентов
        List<Student> Students = new List<Student>();

        public Form1()
        {
            InitializeComponent();

            //инициализировать DataGridView начальными данными
            using (StudentContext db = new StudentContext())
            {
                Students = db.Students.ToList();
                StudentsGridView.DataSource = Students;
            }

            //скрыть колонку с id
            StudentsGridView.Columns[0].Visible = false;
            //выбрать по умолчанию первый элемент в ComboBox
            SearchType.SelectedIndex = 0;
        }

        //добавить объект
        private void CreateButton_Click(object sender, EventArgs e)
        {
            try
            {
                //открыть форму для создания
                Edit create = new Edit("Create");
                DialogResult result = create.ShowDialog(this);
                if (result == DialogResult.Cancel)
                    return;
                if (ValidateModel(create))
                {
                    using (StudentContext db = new StudentContext())
                    {
                        //если модель прошла валидацию, то создать объект
                        Student student = new Student
                        {
                            Name = create.NameBox.Text,
                            Surname = create.SurnameBox.Text,
                            BirthYear = Convert.ToInt32(create.YearBox.Text)
                        };
                        //добавить объект и сохранить изменения
                        db.Students.Add(student);
                        db.SaveChanges();
                        //обновить DataGridView
                        RefreshGrid();
                        for (int i = 0; i < StudentsGridView.Rows.Count; i++)
                        {
                            //поиск добавленного студента в DataGridView
                            if (StudentsGridView.Rows[i].Cells[0].Value.Equals(student.Id))
                            {
                                //если найден, то он был успешно добавлен
                                MessageBox.Show("Студент успешно добавлен");
                                return;
                            }
                        }
                        //иначе сгенерировать исключение
                        throw new Exception("Произошла ошибка. Студент не добавлен");
                    }
                }
                else
                {
                    throw new Exception("Модель не валидна");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //удалить объект
        private void DeleteButton_Click(object sender, EventArgs e)
        {
            try
            {

                using (StudentContext db = new StudentContext())
                {
                    //коллекция для студентов, которых необходимо удалить
                    List<Student> students = new List<Student>();
                    StringBuilder question = new StringBuilder("Вы действительно хотите удалить слудеющих студентов:");
                    foreach (DataGridViewRow row in StudentsGridView.SelectedRows)
                    {
                        //для каждой выделенной строки получить id
                        int id = (int)row.Cells[0].Value;
                        //найти по этому id объект
                        Student student = db.Students.Find(id);
                        if (student != null)
                        {
                            //если найден, то добавить в коллекцию   
                            students.Add(student);
                            question.Append("\n" + student.Surname + " " + student.Name);
                        }
                        else
                        {
                            throw new Exception("Ошибка выборки");
                        }
                    }
                    //подтверждение удаления со списком всех объектов
                    DialogResult result = MessageBox.Show(question.ToString() + "?", "Удаление", MessageBoxButtons.OKCancel);
                    if (result == DialogResult.Cancel)
                        return;
                    //удалить выбранные объекты и сохранить изменения
                    db.Students.RemoveRange(students);
                    db.SaveChanges();
                    //обновить DataGridView
                    RefreshGrid();
                    for (int i = 0; i < StudentsGridView.Rows.Count; i++)
                    {
                        foreach (Student s in students)
                        {
                            //если удалённый студент найден в DataGridView, то удаление прошло неудачно
                            if (StudentsGridView.Rows[i].Cells[0].Value.Equals(s.Id))
                            {
                                throw new Exception("Произошла ошибка. Студент не удалён");
                            }
                        }
                    }
                    MessageBox.Show("Студенты успешно удалёны");

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //редактировать объект
        private void EditButton_Click(object sender, EventArgs e)
        {
            try
            {
                //если выделена не одна строка, то ошибка
                if (StudentsGridView.SelectedRows.Count != 1)
                {
                    throw new Exception("Необходимо выбрать одну запись для редактирования!");
                }
                Student student = null;
                using (StudentContext db = new StudentContext())
                {
                    //найти объект по id
                    int id = (int)StudentsGridView.SelectedRows[0].Cells[0].Value;
                    student = db.Students.Find(id);
                }
                //если не найден, то ошибка
                if (student == null)
                {
                    throw new Exception("Студент не найден!");
                }
                //открыть форму для редактирования
                Edit edit = new Edit("Edit", student);
                DialogResult result = edit.ShowDialog(this);
                if (result == DialogResult.Cancel)
                    return;
                if (ValidateModel(edit))
                {
                    using (StudentContext db = new StudentContext())
                    {
                        //если модель прошла валидацию, то меняем все поля у полученного ранее объекта
                        student.Name = edit.NameBox.Text;
                        student.Surname = edit.SurnameBox.Text;
                        student.BirthYear = Convert.ToInt32(edit.YearBox.Text);
                        //пометить объект как изменённый и сохранить
                        db.Entry(student).State = EntityState.Modified;
                        db.SaveChanges();
                        //обновить DataGridView
                        RefreshGrid();
                        for (int i = 0; i < StudentsGridView.Rows.Count; i++)
                        {
                            //найти студента в DataGridView по id
                            if (StudentsGridView.Rows[i].Cells[0].Value.Equals(student.Id))
                            {
                                //если найден, то проверить, были ли изменены все поля
                                DataGridViewCellCollection cells = StudentsGridView.Rows[i].Cells;
                                if (cells[1].Value.ToString() == student.Name &&
                                    cells[2].Value.ToString() == student.Surname &&
                                    (int)cells[3].Value == student.BirthYear)
                                {
                                    MessageBox.Show("Студент успешно отредактирован");
                                    return;
                                }
                                else
                                {
                                    throw new Exception("Произошла ошибка. Информация не отредактирована");
                                }
                            }
                        }
                        throw new Exception("Произошла ошибка. Студент не найден");
                    }
                }
                else
                {
                    throw new Exception("Модель не валидна");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //загрузить в кэш данные из БД и провести поиск(если что-то есть в строке поиска)
        private void RefreshButton_Click(object sender, EventArgs e)
        {
            RefreshGrid();
            Search();
        }

        //искать при вводе в TextBox
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Search();
        }

        //искать при изменении критерия поиска
        private void SearchType_SelectedIndexChanged(object sender, EventArgs e)
        {
            Search();
        }

        //поиск производится в кэше для минимизации обращения к БД
        private void Search()
        {
            //получить строку для поиска и привести её к нижнему регистру
            string searchString = SearchTextBox.Text.ToLower();
            if (!String.IsNullOrEmpty(searchString))
            {
                //если строка не пустая, то, в зависимости от значения ComboBox, провести соответствующий поиск
                switch (SearchType.SelectedIndex)
                {
                    //по имени
                    case 0: StudentsGridView.DataSource = Students.Where(s => s.Name.ToLowerInvariant().Contains(searchString)).ToList(); break;
                    //по фамилии
                    case 1: StudentsGridView.DataSource = Students.Where(s => s.Surname.ToLowerInvariant().Contains(searchString)).ToList(); break;
                    //по дате рождения
                    case 2: StudentsGridView.DataSource = Students.Where(s => s.BirthYear.ToString().Contains(searchString)).ToList(); break;
                }
            }
            else
            {
                //если строка для поиска пустая, вывести все объекты
                StudentsGridView.DataSource = Students;
            }
        }

        //обновить таблицу данных
        private void RefreshGrid()
        {
            using (StudentContext db = new StudentContext())
            {
                Students = db.Students.ToList();
                StudentsGridView.DataSource = Students;
            }
        }

        //валидация модели
        private bool ValidateModel(Edit form)
        {
            //проверить каждое поле
            form.ValidateText(form.NameBox, null);
            form.ValidateText(form.SurnameBox, null);
            form.ValidateYear(form.YearBox, null);
            //для всех TextBox'ов в GroupBox'е проверить наличие ошибок у соответствующего ErrorProvider
            foreach (Control c in form.groupBox1.Controls)
            {
                if (c.GetType() == typeof(TextBox))
                {
                    ErrorProvider provider = (c as TextBox).Tag as ErrorProvider;
                    //если есть хоть одна ошибка, модель не проходит валидацию
                    if (!String.IsNullOrEmpty(provider.GetError(c)))
                        return false;
                }
            }
            return true;
        }
    }
}
