using Microsoft.AspNetCore.Mvc;
using System;
using UrlShortener.Data;
using UrlShortener.Models;

namespace UrlShortener.Controllers
{
    [Route("api/urlshortner")]
    [ApiController]
    public class UrlShortenerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UrlShortenerController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpPost]
        [Route("shorten")]
        public IActionResult ShortenUrl(string url)
        {
            var isExists = _context.ShortenUrls.Any(s => s.OriginalUrl.Equals(url));
            if (isExists) return BadRequest();
            string shortCode;
            while(true)
            {
                shortCode = GenerateShortCode();
                if (!_context.ShortenUrls.Any(s => s.ShortCode.Equals(shortCode)))
                    break;
            }
            ShortUrl shortUrl = new()
            {
                OriginalUrl = url,
                ShortCode = shortCode,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.ShortenUrls.Add(shortUrl);
            _context.SaveChanges();
            return StatusCode(201, shortUrl);
        }
        [HttpGet]
        [Route("shorten/{shortCode}")]
        public IActionResult GetOriginalUrlFromShortCode(string shortCode)
        {
            var shortUrl = _context.ShortenUrls.SingleOrDefault(su => su.ShortCode == shortCode);
            if (shortUrl == null)
                return NotFound();
            shortUrl.AccessCount += 1;
            _context.SaveChanges();
            return Ok(shortUrl);
        }
        [HttpPut]
        [Route("shorten/{shortCode}")]
        public IActionResult ModifyShortUrl(string shortCode, string url)
        {
            var isExists = _context.ShortenUrls.Any(su => su.ShortCode == shortCode);
            if (!isExists) return NotFound();
            var existingShortUrl = _context.ShortenUrls.Where(su => su.ShortCode == shortCode)
                .Single();
            existingShortUrl.OriginalUrl = url;
            existingShortUrl.UpdatedAt = DateTime.UtcNow;
            _context.SaveChanges();
            return Ok(existingShortUrl);
        }
        [HttpDelete]
        [Route("shorten/{shortCode}")]
        public IActionResult DeleteShortUrl(string shortCode)
        {
            var isExists = _context.ShortenUrls.Any(su => su.ShortCode == shortCode);
            if (!isExists) return NotFound();
            _context.ShortenUrls.Remove(_context.ShortenUrls.Where(su => su.ShortCode == shortCode).Single());
            _context.SaveChanges();
            return NoContent();
        }
        [HttpGet]
        [Route("shorten/{shortCode}/stats")]
        public IActionResult GetStatsForShortCode(string shortCode)
        {
            var isExists = _context.ShortenUrls.Any(su => su.ShortCode == shortCode);
            if (!isExists) return NotFound();
            var shortUrl = _context.ShortenUrls.Where(su => su.ShortCode == shortCode).Single();
            return Ok(shortUrl);
        }
        private static string GenerateShortCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, 6)
                                        .Select(s => s[new Random().Next(s.Length)]).ToArray());
        }
    }
}
