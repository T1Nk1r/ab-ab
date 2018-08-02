using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        //定义的假想数据源
        //开奖号码为3个号的
        int prize_number_count = 8;
        //统计多少期
        int summary_count = 60;
        //总球数为10个
        int total_balls_count = 20;

        private void button1_Click(object sender, EventArgs e)
        {


            //初始化 summary_count 期 ball_count 个开奖号码个数的开奖号码集合
            List<List<int>> inits = new List<List<int>>();
            Random rd = new Random();
            for (int i = 0; i < summary_count; i++)
            {
                List<int> a = new List<int>();

                for (int j = 0; j < prize_number_count; j++)
                {
                    a.Add(rd.Next(1, total_balls_count));
                }

                inits.Add(a);
            }

            richTextBox1.AppendText("打印每期中奖号码\r\n");
            //打印出每期中奖号码
            for (int i = 0; i < inits.Count; i++)
            {
                for (int j = 0; j < inits[i].Count; j++)
                {
                    richTextBox1.AppendText(inits[i][j] + " ");
                }
                richTextBox1.AppendText("\r\n");
            }



            //声明一个统计用的2维泛型集合并初始化值为-1
            List<List<int>> summary = new List<List<int>>();

            //有多少个号就生成多少列
            for (int i = 0; i < summary_count; i++)
            {
                List<int> prize_number = new List<int>();

                for (int j = 0; j < total_balls_count; j++)
                {
                    prize_number.Add(-1);
                }
                summary.Add(prize_number);
            }


            richTextBox1.AppendText("把中奖号码赋值到统计集合中\r\n");

            //把每一期的中奖号码赋值到统计集合中
            for (int i = 0; i < summary.Count; i++)
            {
                //有多少中奖号码即循环几次赋值
                for (int k = 0; k < prize_number_count; k++)
                {
                    //Console.Write("中奖值为:" + inits[i][k]);
                    //inits[i][k] 的值为具体第i期的第k个中奖号码值。
                    //inits[0][0] 即为 第1期第1个中奖号码,i为第几期,k为第几个号码 
                    int prize_number = inits[i][k] - 1;
                    //total_balls_count * k 为第k个中奖号的偏移量，因为每个中奖号码都需要占用所有号码的下标位置
                    //int offset = total_balls_count * k;

                    //总偏移量为每一个中奖号码的偏移量加上本上的值。比如：假设总共10个号码
                    //第一个中奖号的偏移量为0*10，中奖号为7.即0+7=7；
                    //第二个中奖号的偏移量为1*10，中奖号为8，即10+8=18；
                    //int total_offset = offset + prize_number;

                    //把每一个中奖号赋值到统计集合中
                    summary[i][prize_number] = prize_number;
                }
            }

            //打印出统计集合所有的值

            richTextBox1.AppendText("打印统计集合初始值\r\n");
            for (int i = 0; i < summary.Count; i++)
            {
                for (int k = 0; k < total_balls_count; k++)
                {
                    richTextBox1.AppendText(summary[i][k] + " ");
                }
                richTextBox1.AppendText("\r\n");
            }

            List<summarydata1> summaries = new List<summarydata1>();

            List<int> totals = new List<int>();
            List<int> continuous = new List<int>();
            List<int> max = new List<int>();
            List<int> avg = new List<int>();

            //按总列数循环总行数次，比较每一列的值
            for (int i = 0; i < total_balls_count; i++)
            {
                //出现总次数
                int total_count = 0;
                //最大遗漏值
                int max_miss_count = 0;
                //最大连出值
                int max_continuous_count = 0;

                //当前遗漏值
                int current_miss_count = 0;
                //当前连出值
                int current_continuous_count = 0;

                int last_row = 0;

                for (int j = 0; j < summary.Count; j++)
                {
                    int current_value = summary[j][i];
                    if (current_value != -1)
                    {
                        //最后出现中奖号行数
                        last_row = j + 1;
                        //更新出现总次数
                        total_count++;
                        //更新当前连出值
                        current_continuous_count++;

                        //更新最大遗漏值
                        if (current_miss_count > max_miss_count)
                        {
                            max_miss_count = current_miss_count;
                        }
                        current_miss_count = 0;
                    }
                    else
                    {
                        //为默认值即为不是中奖号遗漏值
                        //更新当前
                        current_miss_count++;

                        //更新最大连出值
                        if (current_continuous_count > max_continuous_count)
                        {
                            max_continuous_count = current_continuous_count;
                        }
                        current_continuous_count = 0;
                    }

                }

                summarydata1 data = new summarydata1();
                data.row_index = i;
                data.summary_count = new Dictionary<string, int>();
                data.summary_count.Add("total_count", total_count);
                data.summary_count.Add("max_continuous_count", max_continuous_count);
                data.summary_count.Add("max_miss_count", max_miss_count);
                data.summary_count.Add("avg_miss_count", total_count != 0 ? (last_row - total_count) / total_count : 0);
                data.summary_count.Add("last_row", last_row);

                summaries.Add(data);

                totals.Add(total_count);
                continuous.Add(max_continuous_count);
                max.Add(max_miss_count);
                avg.Add(total_count != 0 ? (last_row - total_count) / total_count : 0);

            }

            for (int i = 0; i < summaries.Count; i++)
            {
                richTextBox1.AppendText(string.Format("列:{0}", summaries[i].row_index));
                foreach (System.Collections.Generic.KeyValuePair<string, int> item in summaries[i].summary_count)
                {
                    richTextBox1.AppendText(string.Format("{0}:{1}", item.Key, item.Value));
                }

                richTextBox1.AppendText("\r\n");
            }


            richTextBox1.AppendText(string.Join(" ", totals) + "\r\n");
            richTextBox1.AppendText(string.Join(" ", continuous) + "\r\n");
            richTextBox1.AppendText(string.Join(" ", max) + "\r\n");
            richTextBox1.AppendText(string.Join(" ", avg) + "\r\n");

            List<SummaryData> summaryDatas = new List<SummaryData>();

            SummaryData total_data = new SummaryData();
            SummaryData continue_data = new SummaryData();
            SummaryData max_data = new SummaryData();
            SummaryData avg_data = new SummaryData();

            total_data.game_type_id = 19;
            total_data.expect_no = "2018201";
            total_data.b_no = 0;
            DynmicAssignment(total_data, totals, total_balls_count);
            DynmicAssignment(continue_data, continuous, total_balls_count);
            DynmicAssignment(max_data, max, total_balls_count);
            DynmicAssignment(avg_data, avg, total_balls_count);

            summaryDatas.Add(total_data);
            summaryDatas.Add(continue_data);
            summaryDatas.Add(max_data);
            summaryDatas.Add(avg_data);


            for (int i = 0; i < summaryDatas.Count; i++)
            {
                richTextBox1.AppendText(summaryDatas[i].b_1 + " " + summaryDatas[i].b_2 + " " + summaryDatas[i].b_3 + " " + summaryDatas[i].b_4 + " " + summaryDatas[i].b_5 + " " +
                    summaryDatas[i].b_6 + " " + summaryDatas[i].b_7 + " " + summaryDatas[i].b_8 + " " + summaryDatas[i].b_9 + " " + summaryDatas[i].b_10 + " " +
                    summaryDatas[i].b_11 + " " + summaryDatas[i].b_12 + " " + summaryDatas[i].b_13 + " " + summaryDatas[i].b_14 + " " + summaryDatas[i].b_15 + " " +
                    summaryDatas[i].b_16 + " " + summaryDatas[i].b_17 + " " + summaryDatas[i].b_18 + " " + summaryDatas[i].b_19 + " " + summaryDatas[i].b_20 + "\r\n");
            }

        }

        public SummaryData DynmicAssignment(SummaryData entity, List<int> source, int count)
        {
            Type type = typeof(SummaryData);
            //取得属性集合
            PropertyInfo[] pi = type.GetProperties();
            foreach (PropertyInfo item in pi)
            {
                if (item.Name.Contains("b_"))
                {
                    int result = 0;
                    bool isInt = int.TryParse(item.Name.Split('_')[1], out result);

                    if (isInt && result <= count)
                    {
                        item.SetValue(entity, Convert.ChangeType(source[result - 1], item.PropertyType), null);
                    }
                }
            }

            return entity;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
        }

        private void a()
        {
            //初始化 summary_count 期 ball_count 个开奖号码个数的开奖号码集合
            List<List<int>> inits = new List<List<int>>();
            Random rd = new Random();
            for (int i = 0; i < summary_count; i++)
            {
                List<int> a = new List<int>();

                for (int j = 0; j < prize_number_count; j++)
                {
                    a.Add(rd.Next(1, 7));
                }

                inits.Add(a);
            }

            richTextBox1.AppendText("打印每期中奖号码\r\n");
            //打印出每期中奖号码
            for (int i = 0; i < inits.Count; i++)
            {
                for (int j = 0; j < inits[i].Count; j++)
                {
                    richTextBox1.AppendText(inits[i][j] + " ");
                }
                richTextBox1.AppendText("\r\n");
            }


            //声明一个统计用的2维泛型集合并初始化值为-1
            List<List<int>> summary = new List<List<int>>();

            for (int i = 0; i < summary_count; i++)
            {
                List<int> prize_number = new List<int>();

                //生成 中奖号码个数*总号码个数列
                for (int j = 0; j < total_balls_count * prize_number_count; j++)
                {
                    prize_number.Add(-1);
                }

                summary.Add(prize_number);
            }


            richTextBox1.AppendText("把中奖号码赋值到统计集合中\r\n");

            //把每一期的中奖号码赋值到统计集合中
            for (int i = 0; i < summary.Count; i++)
            {
                //有多少中奖号码即循环几次赋值
                for (int k = 0; k < prize_number_count; k++)
                {
                    //Console.Write("中奖值为:" + inits[i][k]);
                    //inits[i][k] 的值为具体第i期的第k个中奖号码值。
                    //inits[0][0] 即为 第1期第1个中奖号码,i为第几期,k为第几个号码 
                    int prize_number = inits[i][k];
                    //total_balls_count * k 为第k个中奖号的偏移量，因为每个中奖号码都需要占用所有号码的下标位置
                    int offset = total_balls_count * k;

                    //总偏移量为每一个中奖号码的偏移量加上本上的值。比如：假设总共10个号码
                    //第一个中奖号的偏移量为0*10，中奖号为7.即0+7=7；
                    //第二个中奖号的偏移量为1*10，中奖号为8，即10+8=18；
                    int total_offset = offset + prize_number;

                    //把每一个中奖号赋值到统计集合中
                    summary[i][total_offset] = prize_number;
                }
            }

            //打印出统计集合所有的值

            richTextBox1.AppendText("打印统计集合初始值\r\n");
            for (int i = 0; i < summary.Count; i++)
            {
                for (int k = 0; k < total_balls_count * prize_number_count; k++)
                {
                    richTextBox1.AppendText(summary[i][k] + " ");
                }
                richTextBox1.AppendText("\r\n");
            }

            List<summarydata1> summaries = new List<summarydata1>();

            List<int> totals = new List<int>();
            List<int> continuous = new List<int>();
            List<int> max = new List<int>();
            List<int> avg = new List<int>();

            //按总列数循环总行数次，比较每一列的值
            for (int i = 0; i < total_balls_count * prize_number_count; i++)
            {
                //出现总次数
                int total_count = 0;
                //最大遗漏值
                int max_miss_count = 0;
                //最大连出值
                int max_continuous_count = 0;

                //当前遗漏值
                int current_miss_count = 0;
                //当前连出值
                int current_continuous_count = 0;

                int last_row = 0;

                for (int j = 0; j < summary.Count; j++)
                {
                    int current_value = summary[j][i];
                    if (current_value != -1)
                    {
                        //最后出现中奖号行数
                        last_row = j + 1;
                        //更新出现总次数
                        total_count++;
                        //更新当前连出值
                        current_continuous_count++;

                        //更新最大遗漏值
                        if (current_miss_count > max_miss_count)
                        {
                            max_miss_count = current_miss_count;
                        }
                        current_miss_count = 0;
                    }
                    else
                    {
                        //为默认值即为不是中奖号遗漏值
                        //更新当前
                        current_miss_count++;

                        //更新最大连出值
                        if (current_continuous_count > max_continuous_count)
                        {
                            max_continuous_count = current_continuous_count;
                        }
                        current_continuous_count = 0;
                    }

                }

                summarydata1 data = new summarydata1();
                data.row_index = i;
                data.summary_count = new Dictionary<string, int>();
                data.summary_count.Add("total_count", total_count);
                data.summary_count.Add("max_continuous_count", max_continuous_count);
                data.summary_count.Add("max_miss_count", max_miss_count);
                data.summary_count.Add("avg_miss_count", total_count != 0 ? (last_row - total_count) / total_count : 0);
                data.summary_count.Add("last_row", last_row);

                summaries.Add(data);

                totals.Add(total_count);
                continuous.Add(max_continuous_count);
                max.Add(max_miss_count);
                avg.Add(total_count != 0 ? (last_row - total_count) / total_count : 0);

            }

            for (int i = 0; i < summaries.Count; i++)
            {
                richTextBox1.AppendText(string.Format("列:{0}", summaries[i].row_index));
                foreach (System.Collections.Generic.KeyValuePair<string, int> item in summaries[i].summary_count)
                {
                    richTextBox1.AppendText(string.Format("{0}:{1}", item.Key, item.Value));
                }

                richTextBox1.AppendText("\r\n");
            }


            richTextBox1.AppendText(string.Join(" ", totals) + "\r\n");
            richTextBox1.AppendText(string.Join(" ", continuous) + "\r\n");
            richTextBox1.AppendText(string.Join(" ", max) + "\r\n");
            richTextBox1.AppendText(string.Join(" ", avg) + "\r\n");

            List<SummaryData> summaryDatas = new List<SummaryData>();


            for (int i = 0; i < prize_number_count; i++)
            {
                List<int> _totals = totals.Skip(i * total_balls_count).Take(total_balls_count).ToList();
                List<int> _continuous = continuous.Skip(i * total_balls_count).Take(total_balls_count).ToList();
                List<int> _max = max.Skip(i * total_balls_count).Take(total_balls_count).ToList();
                List<int> _avg = avg.Skip(i * total_balls_count).Take(total_balls_count).ToList();

                SummaryData total_data = new SummaryData();
                SummaryData continue_data = new SummaryData();
                SummaryData max_data = new SummaryData();
                SummaryData avg_data = new SummaryData();

                total_data.game_type_id = 19;
                total_data.expect_no = "2018201";
                total_data.b_no = i;
                DynmicAssignment(total_data, _totals, total_balls_count);
                DynmicAssignment(continue_data, _continuous, total_balls_count);
                DynmicAssignment(max_data, _max, total_balls_count);
                DynmicAssignment(avg_data, _avg, total_balls_count);

                summaryDatas.Add(total_data);
                summaryDatas.Add(continue_data);
                summaryDatas.Add(max_data);
                summaryDatas.Add(avg_data);
            }

            for (int i = 0; i < summaryDatas.Count; i++)
            {
                richTextBox1.AppendText(summaryDatas[i].b_1 + " " + summaryDatas[i].b_2 + " " + summaryDatas[i].b_3 + " " + summaryDatas[i].b_4 + " " + summaryDatas[i].b_5 + " " +
                    summaryDatas[i].b_6 + " " + summaryDatas[i].b_7 + " " + summaryDatas[i].b_8 + " " + summaryDatas[i].b_9 + " " + summaryDatas[i].b_10 + " " +
                    summaryDatas[i].b_11 + " " + summaryDatas[i].b_12 + " " + summaryDatas[i].b_13 + " " + summaryDatas[i].b_14 + " " + summaryDatas[i].b_15 + " " +
                    summaryDatas[i].b_16 + " " + summaryDatas[i].b_17 + " " + summaryDatas[i].b_18 + " " + summaryDatas[i].b_19 + " " + summaryDatas[i].b_20 + "\r\n");
            }
        }
    }
}
