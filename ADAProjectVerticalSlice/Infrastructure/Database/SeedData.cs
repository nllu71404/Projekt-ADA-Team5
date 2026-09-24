using ADAProjectAPIVerticalSlice.Entities;
using Microsoft.EntityFrameworkCore;

namespace ADAProjectAPIVerticalSlice.Infrastructure.Database
{
    public static class SeedData
    {
        public static async Task InitializeAsync(
            IServiceProvider services)
        {
            var dbContext =
                services.GetRequiredService<ApplicationDbContext>();


            // ==========================================
            // 1. Opret test Company
            // ==========================================

            var company = await dbContext.Companies
                .FirstOrDefaultAsync(c =>
                    c.CompanyName == "Test Company");

            if (company == null)
            {
                company = new Company
                {
                    CompanyId = Guid.NewGuid(),
                    CompanyName = "Test Company"
                };

                dbContext.Companies.Add(company);

                await dbContext.SaveChangesAsync();
            }


            // ==========================================
            // 2. Opret test User
            // ==========================================

            var user = await dbContext.Users
                .FirstOrDefaultAsync(u =>
                    u.Email == "test@test.dk");

            if (user == null)
            {
                user = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = "test@test.dk",
                    Email = "test@test.dk",
                    FullName = "Test Bruger",

                    CompanyId = company.CompanyId
                };

                dbContext.Users.Add(user);

                await dbContext.SaveChangesAsync();
            }


            // ==========================================
            // 3. Opret Regions
            // ==========================================

            var regions = new[]
            {
            "United Kingdom East",
            "Continental Europe",
            "United Kingdom West",
            "Americas",
            "Asia Pacific",
            "Middle East",
            "Africa",
            "Nordics",
            "Central Europe",
            "Southern Europe"
        };

            foreach (var regionName in regions)
            {
                var regionExists = await dbContext.Regions
                    .AnyAsync(r => r.RegionName == regionName);

                if (!regionExists)
                {
                    dbContext.Regions.Add(new Region
                    {
                        RegionId = Guid.NewGuid(),
                        RegionName = regionName
                    });
                }
            }

            await dbContext.SaveChangesAsync();


            // ==========================================
            // 4. Opret ADA Survey
            // ==========================================

            var surveyId =
                Guid.Parse("22222222-2222-2222-2222-222222222222");

            var survey = await dbContext.Surveys
                .FirstOrDefaultAsync(s =>
                    s.SurveyId == surveyId);

            if (survey == null)
            {
                survey = new Survey
                {
                    SurveyId = surveyId,
                    Title = "ADA",
                    Description =
                        "Standardiseret ADA-survey til måling af systemadoption."
                };

                dbContext.Surveys.Add(survey);

                await dbContext.SaveChangesAsync();
            }


            // ==========================================
            // 5. Opret Themes
            // ==========================================

            var usabilityId =
                Guid.Parse("33333333-3333-3333-3333-333333333333");

            var selfEfficacyId =
                Guid.Parse("44444444-4444-4444-4444-444444444444");

            var enjoymentId =
                Guid.Parse("55555555-5555-5555-5555-555555555555");

            var confirmationId =
                Guid.Parse("66666666-6666-6666-6666-666666666666");

            var organizationalSupportId =
                Guid.Parse("77777777-7777-7777-7777-777777777777");

            var jobFitId =
                Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

            var dataQualityId =
                Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

            var processFitId =
                Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

            var workaroundsId =
                Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");

            var switchingBenefitsId =
                Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");


            var themes = new[]
            {
            new Theme
            {
                ThemeId = usabilityId,
                Title = "Usability",
                Description =
                    "Hvor nemt og intuitivt systemet er at bruge.",
                Freeform =
                    "Har du yderligere kommentarer til systemets brugervenlighed?",
                SurveyId = surveyId
            },

            new Theme
            {
                ThemeId = selfEfficacyId,
                Title = "Self-efficacy",
                Description =
                    "Brugerens oplevelse af egne evner til at bruge systemet.",
                Freeform =
                    "Har du yderligere kommentarer til din oplevelse af at kunne bruge systemet?",
                SurveyId = surveyId
            },

            new Theme
            {
                ThemeId = enjoymentId,
                Title = "Enjoyment",
                Description =
                    "Hvor behageligt og positivt brugeren oplever arbejdet med systemet.",
                Freeform =
                    "Har du yderligere kommentarer til din oplevelse af at arbejde i systemet?",
                SurveyId = surveyId
            },

            new Theme
            {
                ThemeId = confirmationId,
                Title = "Confirmation",
                Description =
                    "Om systemet lever op til brugerens forventninger.",
                Freeform =
                    "Har du yderligere kommentarer til systemets evne til at leve op til dine forventninger?",
                SurveyId = surveyId
            },

            new Theme
            {
                ThemeId = organizationalSupportId,
                Title = "Organizational Support",
                Description =
                    "Den støtte og hjælp brugeren oplever fra organisationen.",
                Freeform =
                    "Har du yderligere kommentarer til den støtte, du får fra organisationen?",
                SurveyId = surveyId
            },

            new Theme
            {
                ThemeId = jobFitId,
                Title = "Job-Fit",
                Description =
                    "Hvor godt systemet passer til brugerens arbejdsopgaver.",
                Freeform =
                    "Har du yderligere kommentarer til systemets understøttelse af dit arbejde?",
                SurveyId = surveyId
            },

            new Theme
            {
                ThemeId = dataQualityId,
                Title = "Data Quality",
                Description =
                    "Brugerens oplevelse af kvaliteten og pålideligheden af data i systemet.",
                Freeform =
                    "Har du yderligere kommentarer til kvaliteten af data i systemet?",
                SurveyId = surveyId
            },

            new Theme
            {
                ThemeId = processFitId,
                Title = "Process-fit",
                Description =
                    "Hvor godt systemet passer til organisationens arbejdsgange.",
                Freeform =
                    "Har du yderligere kommentarer til systemets arbejdsgange?",
                SurveyId = surveyId
            },

            new Theme
            {
                ThemeId = workaroundsId,
                Title = "Workarounds",
                Description =
                    "I hvor høj grad brugeren arbejder uden om systemet.",
                Freeform =
                    "Har du yderligere kommentarer til brugen af andre løsninger ved siden af systemet?",
                SurveyId = surveyId
            },

            new Theme
            {
                ThemeId = switchingBenefitsId,
                Title = "Switching Benefits",
                Description =
                    "Brugerens oplevelse af fordelene ved det nye system sammenlignet med den tidligere løsning.",
                Freeform =
                    "Har du yderligere kommentarer til fordelene ved det nye system?",
                SurveyId = surveyId
            }
        };


            foreach (var theme in themes)
            {
                var themeExists = await dbContext.Themes
                    .AnyAsync(t => t.ThemeId == theme.ThemeId);

                if (!themeExists)
                {
                    dbContext.Themes.Add(theme);
                }
            }

            await dbContext.SaveChangesAsync();


            // ==========================================
            // 6. Opret Questions
            // ==========================================

            var questions = new[]
            {
            // -------------------------
            // Usability
            // -------------------------

            new Question
            {
                QuestionId = Guid.Parse(
                    "81111111-1111-1111-1111-111111111111"),
                QuestionText = "[System] er let at bruge.",
                QuestionType = "Scale",
                QuestionPolarity = "Positive",
                SurveyId = surveyId,
                ThemeId = usabilityId
            },

            new Question
            {
                QuestionId = Guid.Parse(
                    "82222222-2222-2222-2222-222222222222"),
                QuestionText =
                    "Det er nemt at finde de funktioner, jeg har brug for.",
                QuestionType = "Scale",
                QuestionPolarity = "Positive",
                SurveyId = surveyId,
                ThemeId = usabilityId
            },

            new Question
            {
                QuestionId = Guid.Parse(
                    "83333333-3333-3333-3333-333333333333"),
                QuestionText =
                    "Det er en frustrerende oplevelse at bruge [System].",
                QuestionType = "Scale",
                QuestionPolarity = "Reverse",
                SurveyId = surveyId,
                ThemeId = usabilityId
            },


            // -------------------------
            // Self-efficacy
            // -------------------------

            new Question
            {
                QuestionId = Guid.Parse(
                    "84444444-4444-4444-4444-444444444444"),
                QuestionText =
                    "Jeg føler mig sikker på at kunne løse mine opgaver i [System].",
                QuestionType = "Scale",
                QuestionPolarity = "Positive",
                SurveyId = surveyId,
                ThemeId = selfEfficacyId
            },

            new Question
            {
                QuestionId = Guid.Parse(
                    "85555555-5555-5555-5555-555555555555"),
                QuestionText =
                    "Jeg ved selv, hvordan jeg løser de fleste problemer i [System].",
                QuestionType = "Scale",
                QuestionPolarity = "Positive",
                SurveyId = surveyId,
                ThemeId = selfEfficacyId
            },

            new Question
            {
                QuestionId = Guid.Parse(
                    "86666666-6666-6666-6666-666666666666"),
                QuestionText =
                    "Jeg har brug for hjælp for overhovedet at kunne bruge [System].",
                QuestionType = "Scale",
                QuestionPolarity = "Reverse",
                SurveyId = surveyId,
                ThemeId = selfEfficacyId
            },


            // -------------------------
            // Enjoyment
            // -------------------------

            new Question
            {
                QuestionId = Guid.Parse(
                    "87777777-7777-7777-7777-777777777777"),
                QuestionText =
                    "Jeg kan godt lide at arbejde i [System].",
                QuestionType = "Scale",
                QuestionPolarity = "Positive",
                SurveyId = surveyId,
                ThemeId = enjoymentId
            },

            new Question
            {
                QuestionId = Guid.Parse(
                    "88888888-8888-8888-8888-888888888888"),
                QuestionText =
                    "[System] gør mit arbejde mere behageligt.",
                QuestionType = "Scale",
                QuestionPolarity = "Positive",
                SurveyId = surveyId,
                ThemeId = enjoymentId
            },

            new Question
            {
                QuestionId = Guid.Parse(
                    "89999999-9999-9999-9999-999999999999"),
                QuestionText =
                    "At bruge [System] er kedeligt og besværligt.",
                QuestionType = "Scale",
                QuestionPolarity = "Reverse",
                SurveyId = surveyId,
                ThemeId = enjoymentId
            },


            // -------------------------
            // Confirmation
            // -------------------------

            new Question
            {
                QuestionId = Guid.Parse(
                    "90000000-0000-0000-0000-000000000001"),
                QuestionText =
                    "[System] lever op til mine forventninger.",
                QuestionType = "Scale",
                QuestionPolarity = "Positive",
                SurveyId = surveyId,
                ThemeId = confirmationId
            },

            new Question
            {
                QuestionId = Guid.Parse(
                    "90000000-0000-0000-0000-000000000002"),
                QuestionText =
                    "[System] fungerer bedre, end jeg havde regnet med.",
                QuestionType = "Scale",
                QuestionPolarity = "Positive",
                SurveyId = surveyId,
                ThemeId = confirmationId
            },

            new Question
            {
                QuestionId = Guid.Parse(
                    "90000000-0000-0000-0000-000000000003"),
                QuestionText =
                    "[System] skuffer mig i det daglige.",
                QuestionType = "Scale",
                QuestionPolarity = "Reverse",
                SurveyId = surveyId,
                ThemeId = confirmationId
            },


            // -------------------------
            // Organizational Support
            // -------------------------

            new Question
            {
                QuestionId = Guid.Parse(
                    "90000000-0000-0000-0000-000000000004"),
                QuestionText =
                    "Jeg får den træning, jeg har brug for til [System].",
                QuestionType = "Scale",
                QuestionPolarity = "Positive",
                SurveyId = surveyId,
                ThemeId = organizationalSupportId
            },

            new Question
            {
                QuestionId = Guid.Parse(
                    "90000000-0000-0000-0000-000000000005"),
                QuestionText =
                    "Der er god hjælp at hente, hvis jeg sidder fast.",
                QuestionType = "Scale",
                QuestionPolarity = "Positive",
                SurveyId = surveyId,
                ThemeId = organizationalSupportId
            },

            new Question
            {
                QuestionId = Guid.Parse(
                    "90000000-0000-0000-0000-000000000006"),
                QuestionText =
                    "Min organisation lader mig i stikken med [System].",
                QuestionType = "Scale",
                QuestionPolarity = "Reverse",
                SurveyId = surveyId,
                ThemeId = organizationalSupportId
            },


            // -------------------------
            // Job-Fit
            // -------------------------

            new Question
            {
                QuestionId = Guid.Parse(
                    "90000000-0000-0000-0000-000000000007"),
                QuestionText =
                    "[System] passer godt til de opgaver, jeg skal løse.",
                QuestionType = "Scale",
                QuestionPolarity = "Positive",
                SurveyId = surveyId,
                ThemeId = jobFitId
            },

            new Question
            {
                QuestionId = Guid.Parse(
                    "90000000-0000-0000-0000-000000000008"),
                QuestionText =
                    "[System] understøtter mit arbejde effektivt.",
                QuestionType = "Scale",
                QuestionPolarity = "Positive",
                SurveyId = surveyId,
                ThemeId = jobFitId
            },

            new Question
            {
                QuestionId = Guid.Parse(
                    "90000000-0000-0000-0000-000000000009"),
                QuestionText =
                    "[System] passer dårligt til mit arbejde.",
                QuestionType = "Scale",
                QuestionPolarity = "Reverse",
                SurveyId = surveyId,
                ThemeId = jobFitId
            },


            // -------------------------
            // Data Quality
            // -------------------------

            new Question
            {
                QuestionId = Guid.Parse(
                    "90000000-0000-0000-0000-000000000010"),
                QuestionText =
                    "Data i [System] er korrekte og pålidelige.",
                QuestionType = "Scale",
                QuestionPolarity = "Positive",
                SurveyId = surveyId,
                ThemeId = dataQualityId
            },

            new Question
            {
                QuestionId = Guid.Parse(
                    "90000000-0000-0000-0000-000000000011"),
                QuestionText =
                    "Jeg kan stole på de oplysninger, [System] giver mig.",
                QuestionType = "Scale",
                QuestionPolarity = "Positive",
                SurveyId = surveyId,
                ThemeId = dataQualityId
            },

            new Question
            {
                QuestionId = Guid.Parse(
                    "90000000-0000-0000-0000-000000000012"),
                QuestionText =
                    "Jeg støder ofte på fejl i data i [System].",
                QuestionType = "Scale",
                QuestionPolarity = "Reverse",
                SurveyId = surveyId,
                ThemeId = dataQualityId
            },


            // -------------------------
            // Process-fit
            // -------------------------

            new Question
            {
                QuestionId = Guid.Parse(
                    "90000000-0000-0000-0000-000000000013"),
                QuestionText =
                    "Arbejdsgangene i [System] giver mening for mig.",
                QuestionType = "Scale",
                QuestionPolarity = "Positive",
                SurveyId = surveyId,
                ThemeId = processFitId
            },

            new Question
            {
                QuestionId = Guid.Parse(
                    "90000000-0000-0000-0000-000000000014"),
                QuestionText =
                    "[System] passer til den måde, mit arbejde er tilrettelagt på.",
                QuestionType = "Scale",
                QuestionPolarity = "Positive",
                SurveyId = surveyId,
                ThemeId = processFitId
            },

            new Question
            {
                QuestionId = Guid.Parse(
                    "90000000-0000-0000-0000-000000000015"),
                QuestionText =
                    "[System] tvinger mig til at arbejde på en dårligere måde.",
                QuestionType = "Scale",
                QuestionPolarity = "Reverse",
                SurveyId = surveyId,
                ThemeId = processFitId
            },


            // -------------------------
            // Workarounds
            // -------------------------

            new Question
            {
                QuestionId = Guid.Parse(
                    "90000000-0000-0000-0000-000000000016"),
                QuestionText =
                    "Jeg bruger [System] fuldt ud, som det er tænkt.",
                QuestionType = "Scale",
                QuestionPolarity = "Positive",
                SurveyId = surveyId,
                ThemeId = workaroundsId
            },

            new Question
            {
                QuestionId = Guid.Parse(
                    "90000000-0000-0000-0000-000000000017"),
                QuestionText =
                    "Jeg laver ofte tingene uden om [System].",
                QuestionType = "Scale",
                QuestionPolarity = "Reverse",
                SurveyId = surveyId,
                ThemeId = workaroundsId
            },

            new Question
            {
                QuestionId = Guid.Parse(
                    "90000000-0000-0000-0000-000000000018"),
                QuestionText =
                    "Jeg bruger andre løsninger for at få mit arbejde gjort.",
                QuestionType = "Scale",
                QuestionPolarity = "Reverse",
                SurveyId = surveyId,
                ThemeId = workaroundsId
            },


            // -------------------------
            // Switching Benefits
            // -------------------------

            new Question
            {
                QuestionId = Guid.Parse(
                    "90000000-0000-0000-0000-000000000019"),
                QuestionText =
                    "[System] er en forbedring i forhold til det, vi brugte før.",
                QuestionType = "Scale",
                QuestionPolarity = "Positive",
                SurveyId = surveyId,
                ThemeId = switchingBenefitsId
            },

            new Question
            {
                QuestionId = Guid.Parse(
                    "90000000-0000-0000-0000-000000000020"),
                QuestionText =
                    "[System] giver mig fordele, jeg ikke havde før.",
                QuestionType = "Scale",
                QuestionPolarity = "Positive",
                SurveyId = surveyId,
                ThemeId = switchingBenefitsId
            },

            new Question
            {
                QuestionId = Guid.Parse(
                    "90000000-0000-0000-0000-000000000021"),
                QuestionText =
                    "Jeg ville foretrække at gå tilbage til den gamle løsning.",
                QuestionType = "Scale",
                QuestionPolarity = "Reverse",
                SurveyId = surveyId,
                ThemeId = switchingBenefitsId
            }
        };


            foreach (var question in questions)
            {
                var questionExists = await dbContext.Questions
                    .AnyAsync(q => q.QuestionId == question.QuestionId);

                if (!questionExists)
                {
                    dbContext.Questions.Add(question);
                }
            }

            await dbContext.SaveChangesAsync();
        }
    }
}