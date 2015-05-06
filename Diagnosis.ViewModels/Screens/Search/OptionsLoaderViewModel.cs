using AutoMapper;
using Diagnosis.Common;
using Diagnosis.ViewModels.DataTransfer;
using Diagnosis.ViewModels.Search;
using System;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class OptionsLoaderViewModel : ViewModelBase
    {
        private string _buffer;
        private bool _part;
        private bool _showB;
        private SearchViewModel searchVm;
        private OptionsLoader loader;

        static OptionsLoaderViewModel()
        {
            Mapper.CreateMap(typeof(SearchOptions), typeof(SearchOptionsDTO));
        }

        public OptionsLoaderViewModel(SearchViewModel s, OptionsLoader loader)
        {
            this.searchVm = s;
            this.loader = loader;
        }

        public RelayCommand LoadOptionsCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    try
                    {
                        var dto = Buffer.DeserializeDCJson<SearchOptionsDTO>();
                        var opt = loader.LoadFromDTO(dto);
                        searchVm.SetOptions(opt);
                        Buffer = "";
                        PartialLoaded = opt.PartialLoaded; // TODO по всем детям
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
                            Buffer = dto.SerializeDCJson();
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
    }
}