using System;

namespace TetrisGame.Models
{
    /// <summary>
    /// 方块工厂，用于创建随机方块
    /// </summary>
    public static class BlockFactory
    {
        private static readonly Random Random = new Random();
        
        /// <summary>
        /// 创建一个随机方块
        /// </summary>
        public static Block CreateRandomBlock()
        {
            int blockType = Random.Next(8); // 0-7之间的随机数，对应8种不同的方块
            
            return blockType switch
            {
                0 => new IBlock(),
                1 => new OBlock(),
                2 => new TBlock(),
                3 => new LBlock(),
                4 => new JBlock(),
                5 => new SBlock(),
                6 => new ZBlock(),
                7 => new PBlock(), // 新增穿透方块
                _ => new IBlock() // 默认返回I形方块
            };
        }
    }
}