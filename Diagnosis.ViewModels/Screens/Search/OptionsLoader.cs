using AutoMapper;
using Diagnosis.Common;
using Diagnosis.Data.Queries;
using Diagnosis.Models;
using Diagnosis.ViewModels.Screens;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Diagnosis.ViewModels.Search
{
    public class OptionsLoader : ViewModelBase
    {
        private string _buffer;
        private bool _part;
        private bool _showB;
        private SearchViewModel searchVm;
        private ISession session;

        static OptionsLoader()
        {
            Mapper.CreateMap(typeof(SearchOptions), typeof(SearchOptionsDTO));
        }

        public OptionsLoader(ISession session, SearchViewModel s)
        {
            this.searchVm = s;
            this.session = session;
        }

        public RelayCommand LoadOptionsCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    try
                    {
                        var dto = Buffer.DeserializeDC<SearchOptionsDTO>();
                        var opt = Load(session, dto);
                        searchVm.SetOptions(opt);
                        Buffer = "";
                        ShowBuffer = false;
                    }
                    catch (Exception)
                    {

                    }

                }, () => ShowBuffer);
            }
        }

        public RelayCommand SaveOptionsCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (searchVm.RootQueryBlock != null)
                    {
                        var dto = Mapper.Map<SearchOptionsDTO>(searchVm.RootQueryBlock.GetSearchOptions());
                        try
                        {
                            Buffer = dto.SerializeDC();
                        }
                        catch (Exception ex)
                        {
                            Buffer = ex.ToString();
                        }
                        ShowBuffer = true;
                    }
                });
            }
        }

        public RelayCommand OpenBufferCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    ShowBuffer = true;
                });
            }
        }

        public bool ShowBuffer
        {
            get
            {
                return _showB;
            }
            set
            {
                if (_showB != value)
                {
                    _showB = value;
                    OnPropertyChanged(() => ShowBuffer);
                }
            }
        }

        public string Buffer
        {
            get
            {
                return _buffer;
            }
            set
            {
                if (_buffer != value)
                {
                    _buffer = value;
                    OnPropertyChanged(() => Buffer);
                }
            }
        }
        public bool PartialLoaded
        {
            get
            {
                return _part;
            }
            set
            {
                if (_part != value)
                {
                    _part = value;
                    OnPropertyChanged(() => PartialLoaded);
                }
            }
        }
        /// <summary>
        /// Делает опции с реальными сущностями.
        /// </summary>
        public SearchOptions Load(ISession session, SearchOptionsDTO dto)
        {
            var result = new SearchOptions();
            result.GroupOperator = dto.GroupOperator;
            result.SearchScope = dto.SearchScope;
            result.MinAny = dto.MinAny;

            var words = WordQuery.ByTitles(session)(dto.WordsAll.Select(x => x.Title));
            result.WordsAll = new List<Word>(words);

            words = WordQuery.ByTitles(session)(dto.WordsAny.Select(x => x.Title));
            result.WordsAny = new List<Word>(words);

            words = WordQuery.ByTitles(session)(dto.WordsNot.Select(x => x.Title));
            result.WordsNot = new List<Word>(words);

            var mWordTitles = from w in dto.MeasuresAll
                                         .Union(dto.MeasuresAny)
                                         .Select(x => x.Word)
                              where w != null
                              select w.Title;
            var mWords = WordQuery.ByTitles(session)(mWordTitles);

            var uomTitles = from u in dto.MeasuresAll
                                         .Union(dto.MeasuresAny)
                                         .Select(x => x.Uom)
                            where u != null
                            select new { Abbr = u.Abbr, TypeName = u.UomType.Title };
            var uoms = uomTitles.Select(x => UomQuery.ByAbbrAndTypeName(session)(x.Abbr, x.TypeName));

            var mAll = dto.MeasuresAll.Select(x =>
                new MeasureOp(x.Operator, x.DbValue, uoms.FirstOrDefault(u => u.Abbr == x.Uom.Abbr))
                    {
                        Word = x.Word == null ? null : mWords.FirstOrDefault(w => w.Title == x.Word.Title)
                    });
            var mAny = dto.MeasuresAny.Select(x =>
                new MeasureOp(x.Operator, x.DbValue, uoms.FirstOrDefault(u => u.Abbr == x.Uom.Abbr))
                    {
                        Word = x.Word == null ? null : mWords.FirstOrDefault(w => w.Title == x.Word.Title)
                    });

            result.MeasuresAll = new List<MeasureOp>(mAll);
            result.MeasuresAny = new List<MeasureOp>(mAny);

            var cats = CategoryQuery.ByTitles(session)(dto.Categories.Select(x => x.Title));
            result.Categories = new List<HrCategory>(cats);

            dto.Children.ForAll(x =>
            {
                var child = Load(session, x);
                result.Children.Add(child);
            });

            if (result.WordsAll.Count != dto.WordsAll.Count ||
                result.WordsAny.Count != dto.WordsAny.Count ||
                result.WordsNot.Count != dto.WordsNot.Count ||
                result.MeasuresAll.Count != dto.MeasuresAll.Count ||
                result.MeasuresAny.Count != dto.MeasuresAny.Count ||
                result.Categories.Count != dto.Categories.Count
                )
            {
                // чего-то нет на клиенте, запрос не такой, каким был сохранен
                PartialLoaded = true;
                result.PartialLoaded = true;
            }

            return result;
        }
    }
}