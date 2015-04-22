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
        private bool _showB;
        private SearchViewModel searchVm;
        private ISession session;

        static OptionsLoader()
        {
            Mapper.CreateMap(typeof(HrSearchOptions), typeof(SearchOptionsDTO));
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
                    var dto = Buffer.DeserializeDC<SearchOptionsDTO>();
                    var opt = Load(session, dto);
                    searchVm.LoadOptions(opt);

                    Buffer = "";
                    ShowBuffer = false;
                });
            }
        }

        public RelayCommand SaveOptionsCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (searchVm.History.CurrentOptions != null)
                    {
                        var dto = Mapper.Map<SearchOptionsDTO>(searchVm.History.CurrentOptions);
                        Buffer = dto.SerializeDC();
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

        public static HrSearchOptions Load(ISession session, SearchOptionsDTO dto)
        {
            var result = new HrSearchOptions();

            var words = dto.WordsAll.Select(x =>
                WordQuery.ByTitle(session)(x.Title))
                .Where(x => x != null);
            result.WordsAll = new List<Word>(words);

            var cats = dto.Categories.Select(x =>
              CategoryQuery.ByTitle(session)(x.Title))
              .Where(x => x != null);
            result.Categories = new List<HrCategory>(cats);

            return result;
        }
    }
}