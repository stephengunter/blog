using System.Collections.Generic;
using ApplicationCore.DataAccess;
using ApplicationCore.Models;
using ApplicationCore.Specifications;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ApplicationCore.Helpers;

namespace ApplicationCore.Services
{
	public interface IAttachmentsService
	{
		Task<IEnumerable<UploadFile>> FetchAsync();

		Task<IEnumerable<UploadFile>> FetchAsync(PostType postType, int postId = 0);

		Task<IEnumerable<UploadFile>> FetchAsync(PostType postType, IList<int> postIds);

		Task<IEnumerable<UploadFile>> FetchByTypesAsync(ICollection<PostType> postTypes);

		Task<UploadFile> FindByNameAsync(string name, PostType postType, int postId);

		Task<IEnumerable<UploadFile>> FetchByIdsAsync(IList<int> ids);

		Task<UploadFile> CreateAsync(UploadFile attachment);

		Task<UploadFile> GetByIdAsync(int id);

		Task RemoveAsync(UploadFile attachment);

		void CreateMany(ICollection<UploadFile> attachments);

		Task UpdateAsync(UploadFile attachment);

		void UpdateRange(IEnumerable<UploadFile> attachments);

		void DeleteRange(IEnumerable<UploadFile> attachments);

		Task DeleteAsync(UploadFile attachment);

	}

	public class AttachmentsService : IAttachmentsService
	{
		private readonly IDefaultRepository<UploadFile> _uploadFileRepository;

		public AttachmentsService(IDefaultRepository<UploadFile> uploadFileRepository)
		{
			this._uploadFileRepository = uploadFileRepository;
		}

		public async Task<IEnumerable<UploadFile>> FetchAsync() => await _uploadFileRepository.ListAsync(new AttachmentFilterSpecifications());
		

		public async Task<IEnumerable<UploadFile>> FetchAsync(PostType postType, int postId = 0)
		{
			if (postId > 0)
			{
				var filter = new AttachmentFilterSpecifications(postType, postId);
				return await _uploadFileRepository.ListAsync(filter);
			}

			return await _uploadFileRepository.ListAsync(new AttachmentFilterSpecifications(postType));

		}

		public async Task<IEnumerable<UploadFile>> FetchByTypesAsync(ICollection<PostType> postTypes)
			=> await _uploadFileRepository.ListAsync(new AttachmentFilterSpecifications(postTypes));

		public async Task<IEnumerable<UploadFile>> FetchAsync(PostType postType, IList<int> postIds)
		{ 
			var list = await _uploadFileRepository.ListAsync(new AttachmentFilterSpecifications(postType));

			return list.Where(x => postIds.Contains(x.PostId));
		}

		public async Task<UploadFile> FindByNameAsync(string name, PostType postType, int postId)
		{
			var attachments = await FetchAsync(postType, postId);
			if (attachments.IsNullOrEmpty()) return null;

			return attachments.Where(a => a.Name == name).FirstOrDefault();
		}

		public async Task<IEnumerable<UploadFile>> FetchByIdsAsync(IList<int> ids)
		{
			var filter = new AttachmentFilterSpecifications(ids);

			return await _uploadFileRepository.ListAsync(filter);
		}

		public async Task<UploadFile> GetByIdAsync(int id) => await _uploadFileRepository.GetByIdAsync(id);

		public async Task RemoveAsync(UploadFile attachment)
		{
			attachment.Removed = true;
			await _uploadFileRepository.UpdateAsync(attachment);
		}

		public async Task<UploadFile> CreateAsync(UploadFile attachment) => await _uploadFileRepository.AddAsync(attachment);

		public void CreateMany(ICollection<UploadFile> attachments) => _uploadFileRepository.AddRange(attachments);

		public async Task UpdateAsync(UploadFile attachment) => await _uploadFileRepository.UpdateAsync(attachment);

		public async Task DeleteAsync(UploadFile attachment) => await _uploadFileRepository.DeleteAsync(attachment);

		public void UpdateRange(IEnumerable<UploadFile> attachments) => _uploadFileRepository.UpdateRange(attachments);

		public void DeleteRange(IEnumerable<UploadFile> attachments) => _uploadFileRepository.DeleteRange(attachments);

		

	}
}
