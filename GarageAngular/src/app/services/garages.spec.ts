import { TestBed } from '@angular/core/testing';

import { Garages } from './garages';

describe('Garages', () => {
  let service: Garages;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Garages);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
