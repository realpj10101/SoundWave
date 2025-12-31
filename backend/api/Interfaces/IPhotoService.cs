using api.Models;
using MongoDB.Bson;

namespace api.Interfaces;

public interface IPhotoService
{
    public Task<string[]?> AddMainPhotoToDiskAsync(IFormFile file, MainPhoto? photo, ObjectId userId);
    public Task<string[]?> AddPhotoToDiskAsync(IFormFile file, ObjectId userId);

    public Task<bool> DeletePhotoFromDisk(Photo photo);
    public Task<bool> DeleteMainPhotoFormDiskAsync(MainPhoto photo);
}
