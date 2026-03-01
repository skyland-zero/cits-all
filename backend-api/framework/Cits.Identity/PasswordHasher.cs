using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Cits;

public static class PasswordHasher
{
    public static string HashPassword(string password)
    {
        // 生成随机盐值（推荐 128 位）
        var salt = new byte[128 / 8];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        // 使用 PBKDF2 哈希算法
        var hash = KeyDerivation.Pbkdf2(
            password,
            salt,
            KeyDerivationPrf.HMACSHA512, // 使用 HMAC-SHA512 作为伪随机函数
            100_000, // 迭代次数（建议至少 100,000）
            256 / 8 // 输出长度（256 位）
        );

        // 组合盐值和哈希值，存储到数据库
        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    public static bool VerifyPassword(string hashedPassword, string inputPassword)
    {
        // 从存储的字符串中提取盐值和哈希值
        var parts = hashedPassword.Split(':');
        var originalHash = Convert.FromBase64String(parts[1]);
        var salt = Convert.FromBase64String(parts[0]);
        // 对输入的密码进行哈希计算
        var inputHash = KeyDerivation.Pbkdf2(
            inputPassword,
            salt,
            KeyDerivationPrf.HMACSHA512,
            100_000,
            256 / 8
        );

        // 比较哈希值是否一致
        return CryptographicOperations.FixedTimeEquals(inputHash, originalHash);
    }
}