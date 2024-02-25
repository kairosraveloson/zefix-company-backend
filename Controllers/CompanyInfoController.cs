// Controllers/CompanyInfoController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Net.Http.Headers;
using System.Text;

namespace Gestion_des_entreprises_dans_la_registre_de_commerce_de_Suisse
{
    [ApiController]
    [Route("api/v1/")]
    public class CompanyInfoController : ControllerBase
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ApiSettings _apiSettings;
        private readonly ApiCredentials _apiCredentials;
        private readonly CompanyController _companyController;
        private readonly IMongoCollection<CompanyInfo> _companyCollection;


        public CompanyInfoController(IHttpClientFactory clientFactory, IOptions<ApiSettings> apiSettings, IOptions<ApiCredentials> apiCredentials, CompanyController companyController, IMongoDatabase mongoDatabase, MongoDBContext dbContext)
        {
            _clientFactory = clientFactory;
            _apiSettings = apiSettings.Value;
            _apiCredentials = apiCredentials.Value;
            _companyController = companyController ?? throw new ArgumentNullException(nameof(companyController));
            _companyCollection = dbContext.Companies;
        }

        [HttpPost("Post_info")]
        public async Task<bool> Post(CompanyInfo companyInfo)
        {
            try
            {
                if (companyInfo == null)
                {
                    Console.WriteLine("Invalid CompanyInfo provided.");
                    return false;
                }

                if (await GetMergedFiltered(companyInfo.uid) != null)
                {
                    return false;
                }

                await _companyCollection.InsertOneAsync(companyInfo);
                string description = await FilteredLegalFormNameUnique(companyInfo.uid);
                var legalFormFetch = await GetByNameFr(description);
                PatchLegalFormByDescriptionInfoUpdate(companyInfo.uid, legalFormFetch);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting data into MongoDB: {ex.Message}");
                return false;
            }
        }

        [HttpGet("CompanyinfoMerged/FilteredUID/{filterUid}")]
        public async Task<string> GetMergedFiltered(string filterUid)
        {
            try
            {
                var apiResults = await GetAPI().ConfigureAwait(false);
                var dbResults = await GetDB().ConfigureAwait(false);

                var apiUids = apiResults.Select(company => company.uid);
                var dbUids = dbResults.Select(company => company.uid);

                var company = apiResults.Concat(dbResults)
                    .FirstOrDefault(c => c.uid != null && c.uid.Equals(filterUid, StringComparison.OrdinalIgnoreCase));

                if (company != null)
                {
                    return company.uid;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting merged results: {ex.Message}");
                return "Internal Server Error";
            }
        }


        [HttpGet("api/v1/legalFormDescriptionFr/")]
        public async Task<LegalForm> GetByNameFr(string description)
        {
            try
            {
                var client = _clientFactory.CreateClient();

                var authenticationBytes = Encoding.ASCII.GetBytes($"{_apiCredentials.Username}:{_apiCredentials.Password}");
                var base64Authentication = Convert.ToBase64String(authenticationBytes);

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64Authentication);

                var apiUrl = $"{_apiSettings.BaseUrl}{"api/v1/legalForm"}";

                var response = await client.GetFromJsonAsync<IEnumerable<LegalForm>>(apiUrl);

                if (response == null)
                {
                    return null;
                }

                if (!string.IsNullOrEmpty(description))
                {
                    response = response.Where(record => record.name.fr.Contains(description, StringComparison.OrdinalIgnoreCase));
                }

                return response.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }



        [HttpPatch("PatchUpdate/{id}")]
        public async Task<IActionResult> PatchUpdate(string id, [FromBody] CompanyUpdate patchModel)
        {
            try
            {
                if (patchModel == null)
                {
                    return BadRequest("Invalid data");
                }

                var filter = Builders<CompanyInfo>.Filter.Eq(u => u.uid, id);

                var updateDefinition = Builders<CompanyInfo>.Update
                    .Set(u => u.name, patchModel.Name)
                    .Set(u => u.adresse.street, patchModel.Address)
                    .Set(u => u.legalForm.name.fr, patchModel.legalForm)
                    .Set(u => u.legalSeat, patchModel.legalSeat)
                    .Set(u => u.deletionDate, patchModel.deletionDate)
                    ;

                var result = await _companyCollection.UpdateOneAsync(filter, updateDefinition);
                string description = await FilteredLegalFormNameUnique(id);
                var legalFormFetch = await GetByNameFr(description);
                PatchLegalFormByDescriptionInfoUpdate(id, legalFormFetch);

                if (result.ModifiedCount == 0)
                {
                    return NotFound($"User with ID {id} not found.");
                }

                return Ok("User updated successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPatch("UpdateLegal/{id}")]
        public async Task<IActionResult> PatchLegalForm(string id, [FromBody] CompanyLegalUpdate patchModel)
        {
            try
            {
                if (patchModel == null)
                {
                    return BadRequest("Invalid data");
                }


                var filter = Builders<CompanyInfo>.Filter.Eq(u => u.uid, id);

                var updateDefinition = Builders<CompanyInfo>.Update


                    .Set(u => u.legalForm.id, patchModel.id)
                    .Set(u => u.legalForm.uid, patchModel.uid)
                    .Set(u => u.legalForm.name.de, patchModel.name.de)
                    .Set(u => u.legalForm.name.fr, patchModel.name.fr)
                    .Set(u => u.legalForm.name.it, patchModel.name.it)
                    .Set(u => u.legalForm.name.en, patchModel.name.en)
                    .Set(u => u.legalForm.shortName.de, patchModel.shortName.de)
                    .Set(u => u.legalForm.shortName.fr, patchModel.shortName.fr)
                    .Set(u => u.legalForm.shortName.it, patchModel.shortName.it)
                    .Set(u => u.legalForm.shortName.en, patchModel.shortName.en)
                    .Set(u => u.deletionDate, patchModel.deletionDate)
                    ;

                var result = await _companyCollection.UpdateOneAsync(filter, updateDefinition);

                if (result.ModifiedCount == 0)
                {
                    return NotFound($"User with ID {id} not found.");
                }

                return Ok("User updated successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPatch("UpdateLegal/Description")]
        public async Task<IActionResult> PatchLegalFormByDescription(string id, [FromBody] CompanyLegalUpdate patchModel)
        {
            try
            {
                if (patchModel == null)
                {
                    return BadRequest("Invalid data");
                }


                var filter = Builders<CompanyInfo>.Filter.Eq(u => u.uid, id);

                var updateDefinition = Builders<CompanyInfo>.Update
                    .Set(u => u.legalForm.id, patchModel.id)
                    .Set(u => u.legalForm.uid, patchModel.uid)
                    .Set(u => u.legalForm.name.de, patchModel.name.de)
                    .Set(u => u.legalForm.name.fr, patchModel.name.fr)
                    .Set(u => u.legalForm.name.it, patchModel.name.it)
                    .Set(u => u.legalForm.name.en, patchModel.name.en)
                    .Set(u => u.legalForm.shortName.de, patchModel.shortName.de)
                    .Set(u => u.legalForm.shortName.fr, patchModel.shortName.fr)
                    .Set(u => u.legalForm.shortName.it, patchModel.shortName.it)
                    .Set(u => u.legalForm.shortName.en, patchModel.shortName.en)
                    .Set(u => u.deletionDate, patchModel.deletionDate)
                    ;

                var result = await _companyCollection.UpdateOneAsync(filter, updateDefinition);

                if (result.ModifiedCount == 0)
                {
                    return NotFound($"User with ID {id} not found.");
                }

                return Ok("User updated successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPatch("UpdateLegal/DescriptionInfoUpdate")]
        public async Task<IActionResult> PatchLegalFormByDescriptionInfoUpdate(string id, [FromBody] LegalForm patchModel)
        {
            try
            {
                if (patchModel == null)
                {
                    return BadRequest("Invalid data");
                }


                var filter = Builders<CompanyInfo>.Filter.Eq(u => u.uid, id);

                var updateDefinition = Builders<CompanyInfo>.Update
                    .Set(u => u.legalForm.id, patchModel.id)
                    .Set(u => u.legalForm.uid, patchModel.uid)
                    .Set(u => u.legalForm.name.de, patchModel.name.de)
                    .Set(u => u.legalForm.name.fr, patchModel.name.fr)
                    .Set(u => u.legalForm.name.it, patchModel.name.it)
                    .Set(u => u.legalForm.name.en, patchModel.name.en)
                    .Set(u => u.legalForm.shortName.de, patchModel.shortName.de)
                    .Set(u => u.legalForm.shortName.fr, patchModel.shortName.fr)
                    .Set(u => u.legalForm.shortName.it, patchModel.shortName.it)
                    .Set(u => u.legalForm.shortName.en, patchModel.shortName.en)
                    ;

                var result = await _companyCollection.UpdateOneAsync(filter, updateDefinition);

                if (result.ModifiedCount == 0)
                {
                    return NotFound($"User with ID {id} not found.");
                }

                return Ok("User updated successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }


        [HttpGet("CompanyinfoAPI")]
        public async Task<IEnumerable<CompanyInfo>> GetAPI()
        {
            var uids = await _companyController.Get().ConfigureAwait(false);
            var detailsList = new List<CompanyInfo>();

            await Task.WhenAll(uids.Select(async uid =>
            {
                var client = _clientFactory.CreateClient();

                var authenticationBytes = Encoding.ASCII.GetBytes($"{_apiCredentials.Username}:{_apiCredentials.Password}");
                var base64Authentication = Convert.ToBase64String(authenticationBytes);

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64Authentication);

                var apiUrl = $"{_apiSettings.BaseUrl}{"api/v1/company/uid/"}{uid}";

                try
                {
                    var companyInfo = await client.GetFromJsonAsync<IEnumerable<CompanyObject>>(apiUrl);

                    if (companyInfo != null)
                    {
                        var details = companyInfo
                            .Select(info => new CompanyInfo
                            {
                                uid = info.Uid,
                                name = info.name,
                                adresse = new CompanyAdresse
                                {
                                    street = info.address?.street,
                                    houseNumber = info.address?.houseNumber
                                },
                                legalSeat = info.legalSeat,
                                legalForm = info.legalForm,
                                deletionDate = info.deletionDate
                            })
                            .Where(info => !string.IsNullOrEmpty(info.uid))
                            .FirstOrDefault();

                        detailsList.Add(details);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching details for UID {uid}: {ex.Message}");
                }
            }));

            return detailsList;
        }


        [HttpGet("CompanyinfoDB")]
        public async Task<List<CompanyInfo>> GetDB()
        {
            var projection = Builders<CompanyInfo>.Projection
                .Include(c => c.uid)
                  .Include(c => c.name)
                    .Include(c => c.adresse.street)
                    .Include(c => c.adresse.houseNumber)
                .Include(c => c.legalSeat)
                .Include(c => c.legalForm.id)
                .Include(c => c.legalForm.uid)
                .Include(c => c.legalForm.name.de)
                .Include(c => c.legalForm.name.fr)
                .Include(c => c.legalForm.name.it)
                .Include(c => c.legalForm.name.en)
                .Include(c => c.legalForm.shortName.de)
                .Include(c => c.legalForm.shortName.fr)
                .Include(c => c.legalForm.shortName.it)
                .Include(c => c.legalForm.shortName.en)
                .Include(c => c.deletionDate)
                .Exclude("_id");

            var companyUIDs = await _companyCollection
                .Find(_ => true)
                .Project<CompanyInfo>(projection)
                .ToListAsync();

            var uidList = companyUIDs.Select(info => new CompanyInfo
            {
                uid = info.uid,
                name = info.name,
                adresse = new CompanyAdresse
                {
                    street = info.adresse?.street,
                    houseNumber = info.adresse?.houseNumber
                },
                legalSeat = info.legalSeat,
                legalForm = new LegalForm
                {
                    id = info.legalForm.id,
                    uid = info.legalForm.uid,
                    name = new LanguagesOptions
                    {
                        de = info.legalForm.name.de,
                        fr = info.legalForm.name.fr,
                        it = info.legalForm.name.it,
                        en = info.legalForm.name.en
                    },
                    shortName = new LanguagesOptions
                    {
                        de = info.legalForm.shortName.de,
                        fr = info.legalForm.shortName.fr,
                        it = info.legalForm.shortName.it,
                        en = info.legalForm.shortName.en
                    }
                },
                deletionDate = info.deletionDate
            }).ToList();

            return uidList;
        }

        [HttpDelete("DeleteCompany/{uid}")]
        public async Task<IActionResult> DeleteCompany(string uid)
        {
            try
            {
                var filter = Builders<CompanyInfo>.Filter.Eq(c => c.uid, uid);
                var result = await _companyCollection.DeleteOneAsync(filter);

                if (result.DeletedCount > 0)
                {
                    return Ok($"Company with UID {uid} deleted successfully.");
                }
                else
                {
                    return NotFound($"Company with UID {uid} not found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting company: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }


        [HttpGet("CompanyinfoMerged")]
        public async Task<IEnumerable<CompanyInfo>> GetMerged()
        {
            var apiResults = await GetAPI().ConfigureAwait(false);
            var dbResults = await GetDB().ConfigureAwait(false);

            var mergedResults = apiResults.Concat(dbResults);

            return mergedResults;
        }

        [HttpGet("CompanyinfoMerged/ID")]
        public async Task<IEnumerable<string>> GetMergedID()
        {
            try
            {
                var apiResults = await GetAPI().ConfigureAwait(false);
                var dbResults = await GetDB().ConfigureAwait(false);

                var apiUids = apiResults.Select(company => company.uid);
                var dbUids = dbResults.Select(company => company.uid);

                var mergedUids = apiUids.Concat(dbUids).Distinct();

                return mergedUids.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting merged results: {ex.Message}");
                return new List<string>();
            }
        }



        [HttpGet("CompanyinfoMerged/FilteredLegalFormName")]
        public async Task<IActionResult> FilteredLegalFormName(string filterUid)
        {
            try
            {
                var apiResults = await GetAPI().ConfigureAwait(false);
                var dbResults = await GetDB().ConfigureAwait(false);

                var company = apiResults.Concat(dbResults)
                    .FirstOrDefault(c => c.uid != null && c.uid.Equals(filterUid, StringComparison.OrdinalIgnoreCase));

                if (company != null)
                {
                    var legalFormInfo = new LegalForm
                    {
                        id = company.legalForm.id,
                        uid = company.legalForm.uid,
                        name = new LanguagesOptions
                        {
                            de = company.legalForm.name.de,
                            fr = company.legalForm.name.fr,
                            it = company.legalForm.name.it,
                            en = company.legalForm.name.en
                        },
                        shortName = new LanguagesOptions
                        {
                            de = company.legalForm.shortName.de,
                            fr = company.legalForm.shortName.fr,
                            it = company.legalForm.shortName.it,
                            en = company.legalForm.shortName.en
                        }
                    };

                    return Ok(legalFormInfo);
                }
                else
                {
                    return NotFound($"Company not found with the specified uid: {filterUid}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting merged result: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("CompanyinfoMerged/FilteredLegalFormNameUnique")]
        public async Task<string> FilteredLegalFormNameUnique(string filterUid)
        {
            try
            {
                var apiResults = await GetAPI().ConfigureAwait(false);
                var dbResults = await GetDB().ConfigureAwait(false);

                var company = apiResults.Concat(dbResults)
                    .FirstOrDefault(c => c.uid != null && c.uid.Equals(filterUid, StringComparison.OrdinalIgnoreCase));

                if (company != null)
                {
                    var legalFormNameFr = company.legalForm.name.fr;
                    return legalFormNameFr;
                }
                else
                {
                    return $"Company not found with the specified uid: {filterUid}";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting merged result: {ex.Message}");
                return "Internal Server Error";
            }
        }

    }
}

