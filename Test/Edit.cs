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
    public partial class Edit : Form
    {
        public Edit()
        {
            InitializeComponent();
        }

        //для создания объекта
        public Edit(string title): this()
        {
            //задать заголовок окна и текст кнопки в зависимости от опреции(Создать/Редактировать)
            Text = title;
            OK_button.Text = title;

            //Для всех TextBox'ов в GroupBox'е добавить через свойство Tag ErrorProvider для валидации поля
            foreach (Control c in groupBox1.Controls)
            {
                if (c.GetType() == typeof(TextBox))
                    c.Tag = new ErrorProvider() { BlinkStyle = ErrorBlinkStyle.NeverBlink };
            }
        }

        //для редактирования объекта
        public Edit(string title, Student model): this(title)
        {
            //если ссылка на объект пустая, генерируем соответствующее исключение
            if (model != null)
            {
                NameBox.Text = model.Name;
                SurnameBox.Text = model.Surname;
                YearBox.Text = model.BirthYear.ToString();
            }
            else
                throw new ArgumentException();
        }

        //валидация года
        internal void ValidateYear(object sender, CancelEventArgs e)
        {
            //получаем ErrorProvider для текущего TextBox'а через свойство Tag 
            ErrorProvider provider = ((sender as TextBox).Tag as ErrorProvider);
            string inputText = (sender as TextBox).Text;
            int year = 0;
            //проверка на пустую строку
            if (String.IsNullOrWhiteSpace(inputText))
            {
                provider.SetError(sender as Control, "Поле не может быть пустым!");
            }
            //проверка на числовое значение
            else if (!Int32.TryParse(inputText, out year))
            {
                provider.SetError(sender as Control, "Может содержать только буквы!");
            }
            //проверка на диапазон от 1900 по текущий год
            else if (year < 1900 || year > DateTime.Now.Year)
            {
                provider.SetError(sender as Control, "Некорректный год!");
            }
            //если валидация пройдена, очистить существующие ошибки текущего ErrorProvider'a
            else provider.Clear();
        }

        //валидация текстовых данных
        internal void ValidateText(object sender, CancelEventArgs e)
        {
            //получаем ErrorProvider для текущего TextBox'а через свойство Tag 
            ErrorProvider provider = ((sender as TextBox).Tag as ErrorProvider);
            string inputText = (sender as TextBox).Text;
            //разрешены только буквы
            string pattern = "^[а-яёa-z]+$";
            //проверка на пустую строку
            if (String.IsNullOrWhiteSpace(inputText))
            {
                provider.SetError(sender as Control, "Поле не может быть пустым!");
            }
            //проверка на соответствие регулярному выражению
            else if (!Regex.IsMatch(inputText, pattern, RegexOptions.IgnoreCase))
            {
                provider.SetError(sender as Control, "Может содержать только буквы!");
            }
            //если валидация пройдена, очистить существующие ошибки текущего ErrorProvider'a
            else provider.Clear();
        }
    }
}
