﻿import { Component, ElementRef, Input, OnInit } from '@angular/core';
import * as ol from 'openlayers';
import { CrashService } from './shared';
import { OptionsService } from '../options.service';

@Component({
    selector: 'event-map',
    template: `<div id={{mapId}}></div>`
})
export class EventMapComponent implements OnInit {
    @Input() mapId: string;

    private olMap: ol.Map;
    private olView: ol.View;
    private olExtent: ol.Extent;

    constructor(
        private element: ElementRef,
        private crashService: CrashService,
        optionService: OptionsService) {
        optionService.getOptions().subscribe(options => {
            this.olExtent = options.mapExtent as ol.Extent;
        });
    }

    ngOnInit() {
        let query: any = {
            dateRange: { startDate: '2017-04-01', endDate: '2017-04-07' }
        };
        this.crashService.getCrashPoints(query).subscribe(pointColl => {
            let features = pointColl.points.map(point => new ol.Feature(new ol.geom.Point(ol.proj.fromLonLat([point.x, point.y]))));

            let source = new ol.source.Vector({
                features: features
            });

            let clusterSource = new ol.source.Cluster({
                distance: 100,
                source: source
            });

            let styleCache = {};
            let clusters = new ol.layer.Vector({
                source: clusterSource,
                style: function (feature) {
                    let size = feature.get('features').length as number;
                    if (pointColl.sampleMultiplier) {
                        size = Math.round(size * pointColl.sampleMultiplier);
                    }
                    let style = (styleCache as any)[size];
                    if (!style) {
                        style = new ol.style.Style({
                            image: new ol.style.Circle({
                                radius: 10,
                                stroke: new ol.style.Stroke({
                                    color: '#fff'
                                }),
                                fill: new ol.style.Fill({
                                    color: '#3399CC'
                                })
                            }),
                            text: new ol.style.Text({
                                text: size.toString(),
                                fill: new ol.style.Fill({
                                    color: '#fff'
                                })
                            })
                        });
                        (styleCache as any)[size] = style;
                    }
                    return style;
                }
            });

            let raster = new ol.layer.Tile({
                source: new ol.source.OSM()
            });

            this.olView = new ol.View({
                center: [0, 0],
                zoom: 2,
                extent: this.olExtent
            });

            this.olMap = new ol.Map({
                layers: [raster, clusters],
                target: this.element.nativeElement.firstElementChild,
                view: this.olView
            });

            // zoom to extent
            this.olView.fit(this.olExtent, this.olMap.getSize());
        });
    }
}
