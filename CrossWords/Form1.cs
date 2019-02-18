using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

namespace CrossWords
{
    public partial class Form1 : Form
    {
        string fileText;
        List<Word> words = new List<Word>();
        List<Word> wordsInCross = new List<Word>();
        int crossSize = 30;
        char[,] cross; 

        public Form1()
        {
            cross = new char[crossSize, crossSize];

            InitializeComponent();
            table.AllowUserToResizeColumns = false;
            table.AllowUserToResizeRows = false;
            table.ColumnCount = crossSize;
            table.Rows.Add(crossSize);
            table.ColumnHeadersVisible = false;
            table.RowHeadersVisible = false;
            for (int i = 0; i < crossSize; i++)
            {
                table.Columns[i].Width = 15;
                table.Rows[i].Height = 15;
            }
            
        }


        private void loadCrossBtn(object sender, EventArgs e)
        {
            //Получаем слова с вопросами из файла
            if (openFileDialog1.ShowDialog() != DialogResult.Cancel)
            {
                
                fileText = File.ReadAllText(openFileDialog1.FileName,Encoding.GetEncoding(1251));
                Regex reg1 = new Regex("-.+\n.+");
                MatchCollection matches = reg1.Matches(fileText);
                string[] cur;

                if (matches.Count != 0)
                {
                    foreach(Match match in matches)
                    {
                        cur = (match.Value.Substring(1)).Split('\n');
                        words.Add(new Word(cur[0].Substring(0,cur[0].Length-1), cur[1]));
                    }
                }

            }

            //Добавляем первое слово в кроссворд
            wordsInCross.Add(words[0]);
            words.RemoveAt(0);
            insertWord(wordsInCross[0], false, crossSize/2, crossSize/2, -1);
            wordsInCross[0].IsVert = false;
            wordsInCross[0].StartX = crossSize / 2;
            wordsInCross[0].StartY = crossSize / 2;
            updateTable();

            Word curWord;
            Word curWordInCross;
            int[] indexesInterception;
            while (words.Count != 0)
            {
                curWord = words[0];
                curWordInCross = wordsInCross[(new Random().Next(0, wordsInCross.Count))];

                //Ищем индексы пересечения
                indexesInterception = findIndexIntercept(curWord, curWordInCross);

                //Если нашли пересечение, пытаемся вставить
                if (indexesInterception.Length != 0)
                {
                    if (insertWord(
                        curWord,
                        !curWordInCross.IsVert,
                        !curWordInCross.IsVert ? curWordInCross.StartX + indexesInterception[1] : curWordInCross.StartX - indexesInterception[0],
                        !curWordInCross.IsVert ? curWordInCross.StartY - indexesInterception[0] : curWordInCross.StartY + indexesInterception[0],
                        indexesInterception[0]
                        ))
                    {
                        curWord.IsVert = !curWordInCross.IsVert;
                        curWord.StartX = curWordInCross.IsVert ? curWordInCross.StartX + indexesInterception[1] : curWordInCross.StartX - indexesInterception[0];
                        curWord.StartY = curWordInCross.IsVert ? curWordInCross.StartY - indexesInterception[0]: curWordInCross.StartY + indexesInterception[0];
                        wordsInCross.Add(curWord);
                        words.Remove(curWord);
                    }
                }
            }

            updateTable();

            //colorCross();
        }

        //Ищет пересечение двух слов. Если находит, то первое число - индекс пересечения первого слова, второе - второго
        int[] findIndexIntercept(Word cur, Word cross)
        {
            for (int i = 0;i < cur.WordStr.Length;i++)
            {
                if (cross.WordStr.IndexOf(cur.WordStr[i]) != -1)
                {
                    //Проверка на соседние слова
                    if (checkWordInserting(
                    cur,
                    !cross.IsVert,
                    !cross.IsVert ? cross.StartX + cross.WordStr.IndexOf(cur.WordStr[i]) : cross.StartX - i,
                    !cross.IsVert ? cross.StartY - i : cross.StartY + i,
                    i))
                    {
                        return new int[] { i, cross.WordStr.IndexOf(cur.WordStr[i]) };
                    }
                }
            }
            return new int[0];
        }

        bool checkWordInserting(Word word, bool isVert, int startX, int startY, int indexIntercept)
        {
            indexIntercept = isVert ? startY + indexIntercept : startX + indexIntercept;

            if ((crossSize - 1) - (isVert ? startY : startX + word.WordStr.Length) <= 0 || startX < 0 || startY < 0) return false; //Если выходит за границы, то низя
            else
            {

                //Проверка на соседние слова и на замещение другого слова
                for (int i = isVert ? startY : startX; i < (isVert ? startY : startX) + word.WordStr.Length; i++)
                {
                    if (isVert) { if (i != indexIntercept && (cross[startX - 1, i] != 0 || cross[startX + 1, i] != 0 || cross[startX,i] != 0)) return false; }
                    else if (i != indexIntercept && (cross[i, startY - 1] != 0 || cross[i, startY + 1] != 0 || cross[i,startY] != 0)) return false;

                }


                return true;
            }
        }

        //Вставка слова в массив, с проверками
        bool insertWord(Word word, bool isVert, int startX, int startY, int indexIntercept)
        {
            indexIntercept = isVert ? startY + indexIntercept : startX + indexIntercept;
            if ((crossSize-1) - (isVert ? startY : startX + word.WordStr.Length) <= 0) return false; //Если выходит за границы, то низя
            else
            {
                
                //Проверка на соседние слова
                for (int i = isVert ? startY : startX; i< (isVert ? startY : startX) + word.WordStr.Length; i++)
                {
                    if (isVert) { if (i != indexIntercept && (cross[startX - 1, i] != 0 || cross[startX + 1, i] != 0)) return false; }
                    else if (i != indexIntercept && (cross[i, startX - 1] != 0 || cross[i, startX + 1] != 0)) return false;
                }
                int index = 0;
                //Все проверки пройдены, можно вставлять
                for (int i = isVert ? startY : startX; i < (isVert ? startY + word.WordStr.Length : startX + word.WordStr.Length); i++)
                {
                    if (isVert) cross[startX, i] = word.WordStr[index++];
                    else cross[i, startY] = word.WordStr[index++];
                }
                return true;
            }
        }

        void updateTable()
        {
            for (int i = 0; i < crossSize; i++)
                for (int j = 0; j < crossSize; j++)
                    table[i, j].Value = cross[i, j];
        }

        //Окрашивает кроссворд и стирает слова
        void colorCross()
        {
            foreach (Word word in wordsInCross)
            {
                if (word.IsVert)
                {
                    for (int i = word.StartX; i < word.StartX + word.WordStr.Length; i++)
                    {
                        table[word.StartY, i].Style.BackColor = Color.Yellow;
                        table[word.StartY, i].Value = "";
                    }
                }
                else
                    for (int i = word.StartY; i < word.StartY + word.WordStr.Length; i++)
                    {
                        table[i, word.StartX].Style.BackColor = Color.Yellow;
                        table[i, word.StartX].Value = "";
                    }
            }
        }
    }

    class Word
    {
        string word;
        string question;
        bool isVert;
        int startX;
        int startY;

        List<int[]> indexesIntercept;

        public Word(string word,string question)
        {
            this.word = word;
            this.question = question;
        }

        public string WordStr { get => word;}
        public bool IsVert { get => isVert; set => isVert = value; }
        public int StartX { get => startX; set => startX = value; }
        public int StartY { get => startY; set => startY = value; }
        public List<int[]> IndexesIntercept { get => indexesIntercept; set => indexesIntercept = value; }
    }
}
