using SYS.CORE.Common;
using SYS.CORE.Infrastructure;
using SYS.CORE.Infrastructure.Data;
using SYS.CORE.Model;
using SYS.CORE.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SYS.CORE.Service
{
    public class LotteryResultAnalysisService
    {

        //开奖号码个数
        private int prize_balls_count = 0;
        //总球数
        private int total_balls_count = 0;
        //是否左偏移
        private int left_offset = 0;

        public int gameTypeId { get; set; }
        public int summaryCount { get; set; }
        public int siteId { get; set; }

        /// <summary>
        /// 此种统计方式为统计  开奖号码个数 * 总号码个数列 的彩种
        /// </summary>
        /// <param name="gameTypeId">彩种ID</param>
        /// <param name="summaryCount">统计期数(30,50,80)</param>
        public void Analysis1(int gameTypeId, int summaryCount)
        {
            using (var conn = (new DapperDb()).DbConnection)
            {
                var analysis = new LotteryResultAnalysisRepository(conn, null);

                List<g_game_type_result> results = analysis.GetLotteryResult(gameTypeId, summaryCount).OrderBy(p => p.open_reward_time_ts).ToList();

                if (results != null && results.Count > 0)
                {

                    #region 
                    string expect_no = results.OrderByDescending(p => p.expect_no).FirstOrDefault().expect_no;
                    //获取最近一期的编号，如果不存在才开始统计
                    bool isExist = analysis.CheckAnalysisIsExist(summaryCount, gameTypeId, expect_no);
                    if (isExist)
                    {
                        return;
                    }

                    List<string> strRewardNos = results.Select(p => p.win_reward_no).ToList();

                    //初始化 summary_count 期 ball_count 个开奖号码个数的开奖号码集合
                    List<List<int>> rewardNos = new List<List<int>>();

                    for (int i = 0; i < strRewardNos.Count; i++)
                    {
                        List<int> rewardNo = new List<int>();

                        string[] strRewardNo = strRewardNos[i].Split(',');
                        for (int j = 0; j < strRewardNo.Length; j++)
                        {
                            rewardNo.Add(Convert.ToInt32(strRewardNo[j]) - left_offset);
                        }
                        rewardNos.Add(rewardNo);
                    }

                    //声明一个统计用的2维泛型集合并初始化值为-1
                    List<List<int>> summary = new List<List<int>>();

                    for (int i = 0; i < summaryCount; i++)
                    {
                        List<int> prize_number = new List<int>();

                        //生成 中奖号码个数*总号码个数列
                        for (int j = 0; j < total_balls_count * prize_balls_count; j++)
                        {
                            prize_number.Add(-1);
                        }

                        summary.Add(prize_number);
                    }


                    try
                    {
                        //把每一期的中奖号码赋值到统计集合中
                        for (int i = 0; i < summary.Count; i++)
                        {
                            //有多少中奖号码即循环几次赋值
                            for (int k = 0; k < prize_balls_count; k++)
                            {
                                //Console.Write("中奖值为:" + inits[i][k]);
                                //inits[i][k] 的值为具体第i期的第k个中奖号码值。
                                //inits[0][0] 即为 第1期第1个中奖号码,i为第几期,k为第几个号码 
                                int prize_number = rewardNos[i][k];
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
                    }
                    catch (Exception ex)
                    {
                        LogHelper.WriteLog(typeof(Exception), "LotteryAnalysisService-> \r\n" + ex.StackTrace + "\r\n " + ex.Message + "\r\n " + ex.InnerException);
                    }


                    List<int> totals = new List<int>();
                    List<int> continuous = new List<int>();
                    List<int> max = new List<int>();
                    List<int> avg = new List<int>();

                    //按总列数循环总行数次，比较每一列的值
                    for (int i = 0; i < total_balls_count * prize_balls_count; i++)
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

                        totals.Add(total_count);
                        continuous.Add(max_continuous_count);
                        max.Add(max_miss_count);
                        avg.Add(total_count != 0 ? (last_row - total_count) / total_count : 0);

                    }

                    List<g_game_ball_result> summaryDatas = new List<g_game_ball_result>();


                    for (int i = 0; i < prize_balls_count; i++)
                    {
                        List<int> _totals = totals.Skip(i * total_balls_count).Take(total_balls_count).ToList();
                        List<int> _continuous = continuous.Skip(i * total_balls_count).Take(total_balls_count).ToList();
                        List<int> _max = max.Skip(i * total_balls_count).Take(total_balls_count).ToList();
                        List<int> _avg = avg.Skip(i * total_balls_count).Take(total_balls_count).ToList();

                        g_game_ball_result total_data = new g_game_ball_result();
                        g_game_ball_result continue_data = new g_game_ball_result();
                        g_game_ball_result max_data = new g_game_ball_result();
                        g_game_ball_result avg_data = new g_game_ball_result();

                        total_data.game_type_id = gameTypeId;
                        total_data.expect_no = expect_no;
                        total_data.b_no = i + 1;
                        total_data.category = 1;

                        avg_data.game_type_id = gameTypeId;
                        avg_data.expect_no = expect_no;
                        avg_data.b_no = i + 1;
                        avg_data.category = 2;

                        max_data.game_type_id = gameTypeId;
                        max_data.expect_no = expect_no;
                        max_data.b_no = i + 1;
                        max_data.category = 3;

                        continue_data.game_type_id = gameTypeId;
                        continue_data.expect_no = expect_no;
                        continue_data.b_no = i + 1;
                        continue_data.category = 4;

                        DynmicAssignment(total_data, _totals, total_balls_count);
                        DynmicAssignment(continue_data, _continuous, total_balls_count);
                        DynmicAssignment(max_data, _max, total_balls_count);
                        DynmicAssignment(avg_data, _avg, total_balls_count);

                        summaryDatas.Add(total_data);
                        summaryDatas.Add(avg_data);
                        summaryDatas.Add(max_data);
                        summaryDatas.Add(continue_data);
                    }

                    if (summaryDatas.Count > 0)
                    {
                        analysis.Insert(summaryCount, summaryDatas);
                    }
                    #endregion
                }

            }
        }

        /// <summary>
        /// 此种统计方式为统计  总号码个数列 的彩种
        /// </summary>
        /// <param name="typeId">彩种ID</param>
        /// <param name="summary_count">统计期数(30,50,80)</param>
        public void Analysis2(int typeId, int summary_count)
        {
            using (var conn = (new DapperDb()).DbConnection)
            {
                var analysis = new LotteryResultAnalysisRepository(conn, null);

                List<g_game_type_result> results = analysis.GetLotteryResult(typeId, summary_count).OrderBy(p => p.open_reward_time_ts).ToList();

                if (results != null && results.Count > 0)
                {

                    #region 
                    string expect_no = results.OrderByDescending(p => p.expect_no).FirstOrDefault().expect_no;
                    //获取最近一期的编号，如果不存在才开始统计
                    bool isExist = analysis.CheckAnalysisIsExist(summary_count, typeId, expect_no);
                    if (isExist)
                    {
                        return;
                    }

                    List<string> strRewardNos = results.Select(p => p.win_reward_no).ToList();

                    //初始化 summary_count 期 ball_count 个开奖号码个数的开奖号码集合
                    List<List<int>> rewardNos = new List<List<int>>();

                    for (int i = 0; i < strRewardNos.Count; i++)
                    {
                        List<int> rewardNo = new List<int>();

                        string[] strRewardNo = strRewardNos[i].Split(',');
                        for (int j = 0; j < strRewardNo.Length; j++)
                        {
                            rewardNo.Add(Convert.ToInt32(strRewardNo[j]) - left_offset);
                        }
                        rewardNos.Add(rewardNo);
                    }

                    //声明一个统计用的2维泛型集合并初始化值为-1
                    List<List<int>> summary = new List<List<int>>();

                    for (int i = 0; i < results.Count; i++)
                    {
                        List<int> prize_number = new List<int>();

                        for (int j = 0; j < total_balls_count; j++)
                        {
                            prize_number.Add(-1);
                        }

                        summary.Add(prize_number);
                    }


                    try
                    {
                        //把每一期的中奖号码赋值到统计集合中
                        for (int i = 0; i < summary.Count; i++)
                        {
                            //有多少中奖号码即循环几次赋值
                            for (int k = 0; k < prize_balls_count; k++)
                            {
                                //inits[i][k] 的值为具体第i期的第k个中奖号码值。
                                //inits[0][0] 即为 第1期第1个中奖号码,i为第几期,k为第几个号码 
                                int prize_number = rewardNos[i][k];

                                //把每一个中奖号赋值到统计集合中
                                summary[i][prize_number] = prize_number;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.WriteLog(typeof(Exception), "LotteryAnalysisService-> \r\n" + ex.StackTrace + "\r\n " + ex.Message + "\r\n " + ex.InnerException);
                    }


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

                        totals.Add(total_count);
                        continuous.Add(max_continuous_count);
                        max.Add(max_miss_count);
                        avg.Add(total_count != 0 ? (last_row - total_count) / total_count : 0);

                    }

                    List<g_game_ball_result> summaryDatas = new List<g_game_ball_result>();

                    g_game_ball_result total_data = new g_game_ball_result();
                    g_game_ball_result continue_data = new g_game_ball_result();
                    g_game_ball_result max_data = new g_game_ball_result();
                    g_game_ball_result avg_data = new g_game_ball_result();

                    total_data.game_type_id = typeId;
                    total_data.expect_no = expect_no;
                    total_data.b_no = 0;
                    total_data.category = 1;

                    avg_data.game_type_id = typeId;
                    avg_data.expect_no = expect_no;
                    avg_data.b_no = 0;
                    avg_data.category = 2;

                    max_data.game_type_id = typeId;
                    max_data.expect_no = expect_no;
                    max_data.b_no = 0;
                    max_data.category = 3;

                    continue_data.game_type_id = typeId;
                    continue_data.expect_no = expect_no;
                    continue_data.b_no = 0;
                    continue_data.category = 4;

                    DynmicAssignment(total_data, totals, total_balls_count);
                    DynmicAssignment(continue_data, continuous, total_balls_count);
                    DynmicAssignment(max_data, max, total_balls_count);
                    DynmicAssignment(avg_data, avg, total_balls_count);

                    summaryDatas.Add(total_data);
                    summaryDatas.Add(avg_data);
                    summaryDatas.Add(max_data);
                    summaryDatas.Add(continue_data);


                    if (summaryDatas.Count > 0)
                    {
                        analysis.Insert(summary_count, summaryDatas);
                    }
                    #endregion
                }

            }
        }

        private g_game_ball_result DynmicAssignment(g_game_ball_result entity, List<int> source, int count)
        {
            Type type = typeof(g_game_ball_result);
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


        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeId"></param>
        /// <returns></returns>
        public bool DifferentLottery(int typeId)
        {
            List<int> lotteries = new List<int>();

            lotteries.Add((int)LotteryTypeEnum.Bj28);
            lotteries.Add((int)LotteryTypeEnum.Cqxync);
            lotteries.Add((int)LotteryTypeEnum.Gdklsf);

            return lotteries.Contains(typeId);
        }

        /// <summary>
        /// 选择具体需要的统计方法
        /// </summary>
        /// <param name="typeId">彩种ID</param>
        /// <param name="summary_count">统计期数</param>
        public void Analysis(int typeId, int summary_count)
        {
            //如果传入的期数值不为30 50 80，就统计30期的
            switch (summary_count)
            {
                case 80:
                    break;
                case 50:
                    break;
                case 30:
                    break;
                default:
                    summary_count = 30;
                    break;
            }

            //根据彩种设置对应的中奖号码个数，计算时是否左偏移，总号码个数
            switch (typeId)
            {
                case 1:
                case 2:
                case 3:
                    prize_balls_count = 10;
                    total_balls_count = 10;
                    left_offset = 1;
                    break;
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 27:
                    prize_balls_count = 5;
                    total_balls_count = 10;
                    break;
                case 12:
                case 13:
                case 14:
                case 15:
                    prize_balls_count = 3;
                    total_balls_count = 6;
                    left_offset = 1;
                    break;
                case 16:
                case 17:
                    prize_balls_count = 8;
                    total_balls_count = 20;
                    left_offset = 1;
                    break;
                case 18:
                case 19:
                case 20:
                    prize_balls_count = 3;
                    total_balls_count = 10;
                    break;
                case 22:
                    prize_balls_count = 5;
                    total_balls_count = 11;
                    left_offset = 1;
                    break;
                default:
                    return;
            }

            if (DifferentLottery(typeId))
            {
                Analysis2(typeId, summary_count);
            }
            else
            {
                Analysis1(typeId, summary_count);
            }


        }

    }
}
